using System;
using System.IO.Pipes;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using PKHeX.Core;

namespace PKHeX.WinForms;

public class DiscordRichPresence : IDisposable
{
    private const string ClientId = "1312345678901234567"; // PKM-Universe Discord App ID placeholder
    private NamedPipeClientStream? _pipe;
    private bool _connected;
    private readonly Timer _updateTimer;
    private SaveFile? _currentSave;
    private string _currentActivity = "Idle";
    private DateTime _startTime;
    private volatile bool _isConnecting;

    public bool IsEnabled { get; set; } = true;

    public DiscordRichPresence()
    {
        _startTime = DateTime.UtcNow;
        _updateTimer = new Timer(UpdatePresence, null, Timeout.Infinite, Timeout.Infinite);
    }

    public void Start()
    {
        if (!IsEnabled || _isConnecting) return;

        // Run connection on background thread to avoid UI freeze
        _isConnecting = true;
        Task.Run(() =>
        {
            try
            {
                TryConnect();
                if (_connected)
                {
                    _updateTimer.Change(0, 15000); // Update every 15 seconds
                }
            }
            catch
            {
                // Discord not running or RPC disabled
            }
            finally
            {
                _isConnecting = false;
            }
        });
    }

    private void TryConnect()
    {
        // Try each pipe with a very short timeout to fail fast
        for (int i = 0; i < 10; i++)
        {
            if (!IsEnabled) return; // Allow early exit

            try
            {
                _pipe = new NamedPipeClientStream(".", $"discord-ipc-{i}", PipeDirection.InOut, PipeOptions.Asynchronous);
                _pipe.Connect(100); // Short 100ms timeout per pipe
                _connected = true;
                Handshake();
                return;
            }
            catch
            {
                _pipe?.Dispose();
                _pipe = null;
            }
        }
    }

    private void Handshake()
    {
        if (_pipe == null || !_connected) return;

        var payload = new
        {
            v = 1,
            client_id = ClientId
        };

        SendMessage(0, JsonSerializer.Serialize(payload));
        ReadMessage(); // Read handshake response
    }

    private void SendMessage(int opcode, string payload)
    {
        if (_pipe == null || !_connected) return;

        try
        {
            var payloadBytes = Encoding.UTF8.GetBytes(payload);
            var packet = new byte[8 + payloadBytes.Length];

            BitConverter.GetBytes(opcode).CopyTo(packet, 0);
            BitConverter.GetBytes(payloadBytes.Length).CopyTo(packet, 4);
            payloadBytes.CopyTo(packet, 8);

            _pipe.Write(packet, 0, packet.Length);
            _pipe.Flush();
        }
        catch
        {
            _connected = false;
        }
    }

    private string? ReadMessage()
    {
        if (_pipe == null || !_connected) return null;

        try
        {
            // Set read timeout to avoid blocking forever
            _pipe.ReadTimeout = 2000;

            var header = new byte[8];
            int bytesRead = _pipe.Read(header, 0, 8);
            if (bytesRead < 8) return null;

            int length = BitConverter.ToInt32(header, 4);
            if (length <= 0 || length > 65536) return null; // Sanity check

            var data = new byte[length];
            _pipe.Read(data, 0, length);

            return Encoding.UTF8.GetString(data);
        }
        catch
        {
            return null;
        }
    }

    public void SetSave(SaveFile? sav)
    {
        _currentSave = sav;
        if (_connected) UpdatePresenceNow();
    }

    public void SetActivity(string activity)
    {
        _currentActivity = activity;
        if (_connected) UpdatePresenceNow();
    }

    private void UpdatePresenceNow()
    {
        if (!IsEnabled || !_connected) return;

        // Run on background thread to avoid any potential blocking
        Task.Run(() => UpdatePresence(null));
    }

    private void UpdatePresence(object? state)
    {
        if (!IsEnabled || !_connected || _pipe == null) return;

        try
        {
            var gameName = _currentSave != null ? GetGameName(_currentSave) : "No save loaded";
            var details = _currentSave != null
                ? $"Editing {_currentSave.OT} - {gameName}"
                : "Ready to edit";

            var presence = new
            {
                cmd = "SET_ACTIVITY",
                args = new
                {
                    pid = Environment.ProcessId,
                    activity = new
                    {
                        state = _currentActivity,
                        details = details,
                        timestamps = new
                        {
                            start = ((DateTimeOffset)_startTime).ToUnixTimeSeconds()
                        },
                        assets = new
                        {
                            large_image = "pkm_universe_logo",
                            large_text = "PKM-Universe",
                            small_image = GetGameIcon(),
                            small_text = gameName
                        },
                        buttons = new[]
                        {
                            new { label = "Get PKM-Universe", url = "https://pkm-universe.com" }
                        }
                    }
                },
                nonce = Guid.NewGuid().ToString()
            };

            SendMessage(1, JsonSerializer.Serialize(presence));
        }
        catch
        {
            // Ignore errors
        }
    }

    private string GetGameName(SaveFile sav)
    {
        return sav.Version switch
        {
            GameVersion.SW => "Pokemon Sword",
            GameVersion.SH => "Pokemon Shield",
            GameVersion.BD => "Pokemon Brilliant Diamond",
            GameVersion.SP => "Pokemon Shining Pearl",
            GameVersion.PLA => "Pokemon Legends: Arceus",
            GameVersion.SL => "Pokemon Scarlet",
            GameVersion.VL => "Pokemon Violet",
            GameVersion.SN => "Pokemon Sun",
            GameVersion.MN => "Pokemon Moon",
            GameVersion.US => "Pokemon Ultra Sun",
            GameVersion.UM => "Pokemon Ultra Moon",
            GameVersion.X => "Pokemon X",
            GameVersion.Y => "Pokemon Y",
            GameVersion.OR => "Pokemon Omega Ruby",
            GameVersion.AS => "Pokemon Alpha Sapphire",
            GameVersion.R => "Pokemon Ruby",
            GameVersion.S => "Pokemon Sapphire",
            GameVersion.E => "Pokemon Emerald",
            GameVersion.FR => "Pokemon FireRed",
            GameVersion.LG => "Pokemon LeafGreen",
            GameVersion.D => "Pokemon Diamond",
            GameVersion.P => "Pokemon Pearl",
            GameVersion.Pt => "Pokemon Platinum",
            GameVersion.HG => "Pokemon HeartGold",
            GameVersion.SS => "Pokemon SoulSilver",
            GameVersion.B => "Pokemon Black",
            GameVersion.W => "Pokemon White",
            GameVersion.B2 => "Pokemon Black 2",
            GameVersion.W2 => "Pokemon White 2",
            GameVersion.GP => "Pokemon Let's Go Pikachu",
            GameVersion.GE => "Pokemon Let's Go Eevee",
            _ => sav.Version.ToString()
        };
    }

    private string GetGameIcon()
    {
        if (_currentSave == null) return "unknown";

        return _currentSave.Generation switch
        {
            1 => "gen1",
            2 => "gen2",
            3 => "gen3",
            4 => "gen4",
            5 => "gen5",
            6 => "gen6",
            7 => "gen7",
            8 => "gen8",
            9 => "gen9",
            _ => "unknown"
        };
    }

    public void Stop()
    {
        IsEnabled = false; // Signal to stop any pending operations
        _updateTimer.Change(Timeout.Infinite, Timeout.Infinite);

        if (_connected && _pipe != null)
        {
            try
            {
                // Clear presence
                var clearPresence = new
                {
                    cmd = "SET_ACTIVITY",
                    args = new { pid = Environment.ProcessId, activity = (object?)null },
                    nonce = Guid.NewGuid().ToString()
                };
                SendMessage(1, JsonSerializer.Serialize(clearPresence));
            }
            catch { }
        }

        _connected = false;
        _pipe?.Dispose();
        _pipe = null;
    }

    public void Dispose()
    {
        Stop();
        _updateTimer.Dispose();
    }
}
