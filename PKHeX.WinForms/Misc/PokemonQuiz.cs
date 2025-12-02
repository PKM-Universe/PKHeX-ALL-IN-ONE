using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using PKHeX.Core;

namespace PKHeX.WinForms;

public class PokemonQuiz : Form
{
    private readonly SaveFile SAV;
    private readonly Random rng = new();
    private Label lblQuestion = null!;
    private Label lblScore = null!;
    private Label lblStreak = null!;
    private Label lblFeedback = null!;
    private Button[] btnAnswers = new Button[4];
    private PictureBox picSprite = null!;
    private ComboBox cmbCategory = null!;
    private ComboBox cmbDifficulty = null!;
    private ProgressBar prgTimer = null!;
    private Timer gameTimer = null!;
    private int score = 0;
    private int streak = 0;
    private int questionsAsked = 0;
    private int correctAnswer = 0;
    private int timeLeft = 0;

    private static readonly string[] Categories = { "Who's That Pokemon?", "Type Quiz", "Evolution Quiz", "Move Quiz", "Ability Quiz", "Stats Quiz", "Generation Quiz" };
    private static readonly string[] Difficulties = { "Easy", "Normal", "Hard", "Expert" };

    public PokemonQuiz(SaveFile sav)
    {
        SAV = sav;
        Text = "Pokemon Quiz";
        Size = new Size(700, 550);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Color.FromArgb(25, 25, 40);
        InitializeUI();
        GenerateQuestion();
    }

    private void InitializeUI()
    {
        var lblTitle = new Label
        {
            Text = "Pokemon Quiz",
            Location = new Point(20, 10),
            AutoSize = true,
            ForeColor = Color.FromArgb(255, 200, 100),
            Font = new Font("Segoe UI", 16F, FontStyle.Bold)
        };

        // Settings
        var lblCategory = new Label { Text = "Category:", Location = new Point(20, 50), AutoSize = true, ForeColor = Color.White };
        cmbCategory = new ComboBox
        {
            Location = new Point(90, 47),
            Width = 180,
            DropDownStyle = ComboBoxStyle.DropDownList,
            BackColor = Color.FromArgb(45, 45, 65),
            ForeColor = Color.White
        };
        cmbCategory.Items.AddRange(Categories);
        cmbCategory.SelectedIndex = 0;
        cmbCategory.SelectedIndexChanged += (s, e) => GenerateQuestion();

        var lblDiff = new Label { Text = "Difficulty:", Location = new Point(300, 50), AutoSize = true, ForeColor = Color.White };
        cmbDifficulty = new ComboBox
        {
            Location = new Point(380, 47),
            Width = 100,
            DropDownStyle = ComboBoxStyle.DropDownList,
            BackColor = Color.FromArgb(45, 45, 65),
            ForeColor = Color.White
        };
        cmbDifficulty.Items.AddRange(Difficulties);
        cmbDifficulty.SelectedIndex = 1;

        // Score
        var grpScore = new GroupBox
        {
            Text = "Score",
            Location = new Point(500, 40),
            Size = new Size(170, 80),
            ForeColor = Color.White
        };

        lblScore = new Label
        {
            Text = "Score: 0",
            Location = new Point(10, 25),
            AutoSize = true,
            ForeColor = Color.Lime,
            Font = new Font("Segoe UI", 12F, FontStyle.Bold)
        };

        lblStreak = new Label
        {
            Text = "Streak: 0",
            Location = new Point(10, 50),
            AutoSize = true,
            ForeColor = Color.Gold,
            Font = new Font("Segoe UI", 10F)
        };

        grpScore.Controls.AddRange(new Control[] { lblScore, lblStreak });

        // Question Area
        var grpQuestion = new GroupBox
        {
            Text = "Question",
            Location = new Point(20, 130),
            Size = new Size(650, 200),
            ForeColor = Color.White
        };

        picSprite = new PictureBox
        {
            Location = new Point(20, 40),
            Size = new Size(128, 128),
            BackColor = Color.FromArgb(35, 35, 55),
            SizeMode = PictureBoxSizeMode.CenterImage
        };

        lblQuestion = new Label
        {
            Text = "Who's That Pokemon?",
            Location = new Point(170, 40),
            Size = new Size(460, 60),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 14F)
        };

        prgTimer = new ProgressBar
        {
            Location = new Point(170, 110),
            Size = new Size(300, 20),
            Maximum = 100,
            Value = 100,
            Style = ProgressBarStyle.Continuous
        };

        lblFeedback = new Label
        {
            Text = "",
            Location = new Point(170, 140),
            Size = new Size(460, 30),
            ForeColor = Color.Lime,
            Font = new Font("Segoe UI", 12F, FontStyle.Bold)
        };

        grpQuestion.Controls.AddRange(new Control[] { picSprite, lblQuestion, prgTimer, lblFeedback });

        // Answers
        var grpAnswers = new GroupBox
        {
            Text = "Answers",
            Location = new Point(20, 340),
            Size = new Size(650, 120),
            ForeColor = Color.White
        };

        for (int i = 0; i < 4; i++)
        {
            int x = (i % 2) * 320 + 15;
            int y = (i / 2) * 45 + 25;

            btnAnswers[i] = new Button
            {
                Text = $"Answer {i + 1}",
                Location = new Point(x, y),
                Size = new Size(300, 40),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(60, 60, 90),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10F),
                Tag = i
            };
            int index = i;
            btnAnswers[i].Click += (s, e) => AnswerClicked(index);
            grpAnswers.Controls.Add(btnAnswers[i]);
        }

        // Controls
        var btnNewQuestion = new Button
        {
            Text = "Skip Question",
            Location = new Point(20, 470),
            Size = new Size(120, 35),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(140, 100, 60),
            ForeColor = Color.White
        };
        btnNewQuestion.Click += (s, e) =>
        {
            streak = 0;
            lblStreak.Text = "Streak: 0";
            GenerateQuestion();
        };

        var btnReset = new Button
        {
            Text = "Reset Score",
            Location = new Point(150, 470),
            Size = new Size(100, 35),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(140, 60, 60),
            ForeColor = Color.White
        };
        btnReset.Click += (s, e) =>
        {
            score = 0;
            streak = 0;
            questionsAsked = 0;
            lblScore.Text = "Score: 0";
            lblStreak.Text = "Streak: 0";
            GenerateQuestion();
        };

        var btnClose = new Button
        {
            Text = "Close",
            Location = new Point(570, 470),
            Size = new Size(100, 35),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(80, 80, 100),
            ForeColor = Color.White
        };
        btnClose.Click += (s, e) => Close();

        // Timer
        gameTimer = new Timer { Interval = 100 };
        gameTimer.Tick += TimerTick;

        Controls.AddRange(new Control[] { lblTitle, lblCategory, cmbCategory, lblDiff, cmbDifficulty, grpScore, grpQuestion, grpAnswers, btnNewQuestion, btnReset, btnClose });
    }

    private void GenerateQuestion()
    {
        lblFeedback.Text = "";
        foreach (var btn in btnAnswers)
        {
            btn.BackColor = Color.FromArgb(60, 60, 90);
            btn.Enabled = true;
        }

        string category = cmbCategory.SelectedItem?.ToString() ?? "Who's That Pokemon?";
        questionsAsked++;

        switch (category)
        {
            case "Who's That Pokemon?":
                GenerateWhosThat();
                break;
            case "Type Quiz":
                GenerateTypeQuiz();
                break;
            case "Evolution Quiz":
                GenerateEvolutionQuiz();
                break;
            case "Move Quiz":
                GenerateMoveQuiz();
                break;
            default:
                GenerateWhosThat();
                break;
        }

        // Start timer
        timeLeft = 100;
        prgTimer.Value = 100;
        gameTimer.Start();
    }

    private void GenerateWhosThat()
    {
        var pokemon = new[] { "Pikachu", "Charizard", "Mewtwo", "Gengar", "Garchomp", "Lucario", "Greninja", "Eevee", "Dragonite", "Tyranitar" };
        var shuffled = pokemon.OrderBy(x => rng.Next()).Take(4).ToArray();
        correctAnswer = rng.Next(4);

        lblQuestion.Text = $"Who's That Pokemon?\n(Hint: It's a popular one!)";

        for (int i = 0; i < 4; i++)
        {
            btnAnswers[i].Text = shuffled[i];
        }
    }

    private void GenerateTypeQuiz()
    {
        var questions = new[]
        {
            ("What type is Pikachu?", new[] { "Electric", "Normal", "Fire", "Water" }, 0),
            ("What type is Charizard?", new[] { "Fire/Flying", "Fire/Dragon", "Fire", "Dragon/Flying" }, 0),
            ("What type is Gengar?", new[] { "Ghost/Poison", "Ghost", "Dark/Ghost", "Poison" }, 0),
            ("What is super effective against Water?", new[] { "Electric", "Fire", "Ice", "Rock" }, 0)
        };

        var q = questions[rng.Next(questions.Length)];
        lblQuestion.Text = q.Item1;
        correctAnswer = q.Item3;

        var answers = q.Item2.OrderBy(x => rng.Next()).ToArray();
        correctAnswer = Array.IndexOf(answers, q.Item2[q.Item3]);

        for (int i = 0; i < 4; i++)
        {
            btnAnswers[i].Text = answers[i];
        }
    }

    private void GenerateEvolutionQuiz()
    {
        var questions = new[]
        {
            ("What does Pikachu evolve into?", new[] { "Raichu", "Pichu", "Plusle", "Pachirisu" }, 0),
            ("What does Eevee NOT evolve into?", new[] { "Pikachu", "Vaporeon", "Jolteon", "Umbreon" }, 0),
            ("What level does Charmander evolve?", new[] { "16", "14", "18", "20" }, 0)
        };

        var q = questions[rng.Next(questions.Length)];
        lblQuestion.Text = q.Item1;
        correctAnswer = q.Item3;

        var answers = q.Item2.OrderBy(x => rng.Next()).ToArray();
        correctAnswer = Array.IndexOf(answers, q.Item2[q.Item3]);

        for (int i = 0; i < 4; i++)
        {
            btnAnswers[i].Text = answers[i];
        }
    }

    private void GenerateMoveQuiz()
    {
        var questions = new[]
        {
            ("What type is Thunderbolt?", new[] { "Electric", "Normal", "Fire", "Water" }, 0),
            ("What move does Pikachu learn at Lv.1?", new[] { "Thunder Shock", "Thunderbolt", "Quick Attack", "Tail Whip" }, 0),
            ("Which move has 100% accuracy?", new[] { "Swift", "Thunder", "Hydro Pump", "Fire Blast" }, 0)
        };

        var q = questions[rng.Next(questions.Length)];
        lblQuestion.Text = q.Item1;
        correctAnswer = q.Item3;

        var answers = q.Item2.OrderBy(x => rng.Next()).ToArray();
        correctAnswer = Array.IndexOf(answers, q.Item2[q.Item3]);

        for (int i = 0; i < 4; i++)
        {
            btnAnswers[i].Text = answers[i];
        }
    }

    private void AnswerClicked(int index)
    {
        gameTimer.Stop();

        foreach (var btn in btnAnswers)
            btn.Enabled = false;

        if (index == correctAnswer)
        {
            score += 10 + streak * 2;
            streak++;
            lblScore.Text = $"Score: {score}";
            lblStreak.Text = $"Streak: {streak}";
            lblFeedback.Text = "Correct!";
            lblFeedback.ForeColor = Color.Lime;
            btnAnswers[index].BackColor = Color.FromArgb(60, 140, 60);
        }
        else
        {
            streak = 0;
            lblStreak.Text = "Streak: 0";
            lblFeedback.Text = $"Wrong! The answer was: {btnAnswers[correctAnswer].Text}";
            lblFeedback.ForeColor = Color.Red;
            btnAnswers[index].BackColor = Color.FromArgb(140, 60, 60);
            btnAnswers[correctAnswer].BackColor = Color.FromArgb(60, 140, 60);
        }

        // Auto next question after delay
        var delayTimer = new Timer { Interval = 1500 };
        delayTimer.Tick += (s, e) =>
        {
            delayTimer.Stop();
            delayTimer.Dispose();
            GenerateQuestion();
        };
        delayTimer.Start();
    }

    private void TimerTick(object? sender, EventArgs e)
    {
        timeLeft -= 2;
        prgTimer.Value = Math.Max(0, timeLeft);

        if (timeLeft <= 0)
        {
            gameTimer.Stop();
            streak = 0;
            lblStreak.Text = "Streak: 0";
            lblFeedback.Text = $"Time's up! The answer was: {btnAnswers[correctAnswer].Text}";
            lblFeedback.ForeColor = Color.Orange;

            foreach (var btn in btnAnswers)
                btn.Enabled = false;
            btnAnswers[correctAnswer].BackColor = Color.FromArgb(60, 140, 60);

            var delayTimer = new Timer { Interval = 2000 };
            delayTimer.Tick += (s, ev) =>
            {
                delayTimer.Stop();
                delayTimer.Dispose();
                GenerateQuestion();
            };
            delayTimer.Start();
        }
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        gameTimer?.Stop();
        gameTimer?.Dispose();
        base.OnFormClosing(e);
    }
}
