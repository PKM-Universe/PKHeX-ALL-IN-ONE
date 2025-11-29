/**
 * Pokemon Cloud Sync Utility
 * Sync collections to cloud storage (Discord, Google Drive, etc.)
 */

const fs = require('fs');
const path = require('path');
const crypto = require('crypto');

class PokemonCloudSync {
    constructor() {
        this.dataPath = path.join(__dirname, '..', 'Json');
        this.syncConfigFile = path.join(this.dataPath, 'cloud_sync_config.json');
        this.syncStatusFile = path.join(this.dataPath, 'cloud_sync_status.json');

        // Sync providers
        this.providers = {
            discord: {
                name: 'Discord',
                maxFileSize: 8 * 1024 * 1024, // 8MB
                supportedTypes: ['json', 'txt']
            },
            local: {
                name: 'Local Backup',
                maxFileSize: Infinity,
                supportedTypes: ['json', 'txt', 'pk9', 'pk8', 'pk7']
            },
            webhookStorage: {
                name: 'Webhook Storage',
                maxFileSize: 25 * 1024 * 1024, // 25MB
                supportedTypes: ['json', 'txt']
            }
        };

        this.encryptionKey = null;
    }

    // Initialize with encryption key
    init(encryptionKey = null) {
        this.encryptionKey = encryptionKey;
        this.loadConfig();
    }

    loadConfig() {
        try {
            if (fs.existsSync(this.syncConfigFile)) {
                return JSON.parse(fs.readFileSync(this.syncConfigFile, 'utf8'));
            }
        } catch (error) {
            console.error('Failed to load sync config:', error);
        }
        return {
            autoSync: false,
            syncInterval: 3600000, // 1 hour
            lastSync: null,
            provider: 'local',
            encryptData: false,
            syncCollections: true,
            syncHistory: true,
            syncSettings: false
        };
    }

    saveConfig(config) {
        try {
            fs.writeFileSync(this.syncConfigFile, JSON.stringify(config, null, 2));
            return true;
        } catch (error) {
            console.error('Failed to save sync config:', error);
            return false;
        }
    }

    loadSyncStatus() {
        try {
            if (fs.existsSync(this.syncStatusFile)) {
                return JSON.parse(fs.readFileSync(this.syncStatusFile, 'utf8'));
            }
        } catch (error) {
            console.error('Failed to load sync status:', error);
        }
        return {
            lastSync: null,
            lastSyncStatus: null,
            syncedFiles: [],
            pendingChanges: [],
            conflicts: []
        };
    }

    saveSyncStatus(status) {
        try {
            fs.writeFileSync(this.syncStatusFile, JSON.stringify(status, null, 2));
            return true;
        } catch (error) {
            console.error('Failed to save sync status:', error);
            return false;
        }
    }

    // Create a backup of all data
    createBackup(options = {}) {
        const config = this.loadConfig();
        const backupData = {
            version: '1.0',
            createdAt: new Date().toISOString(),
            creator: options.userId || 'system',
            collections: {},
            metadata: {}
        };

        // Files to backup
        const filesToBackup = [
            'pokemon_collections.json',
            'living_dex_progress.json',
            'trade_history.json',
            'import_history.json'
        ];

        if (config.syncSettings) {
            filesToBackup.push(
                'pokemon_config.json',
                'pokemon_game_config.json'
            );
        }

        for (const filename of filesToBackup) {
            const filePath = path.join(this.dataPath, filename);
            if (fs.existsSync(filePath)) {
                try {
                    const content = fs.readFileSync(filePath, 'utf8');
                    const data = JSON.parse(content);
                    backupData.collections[filename] = data;
                    backupData.metadata[filename] = {
                        size: content.length,
                        hash: this.hash(content)
                    };
                } catch (e) {
                    console.error(`Failed to read ${filename}:`, e);
                }
            }
        }

        // Encrypt if configured
        let finalData = JSON.stringify(backupData, null, 2);
        if (config.encryptData && this.encryptionKey) {
            finalData = this.encrypt(finalData);
            backupData.encrypted = true;
        }

        return {
            success: true,
            data: finalData,
            size: finalData.length,
            fileCount: Object.keys(backupData.collections).length,
            timestamp: backupData.createdAt
        };
    }

    // Restore from backup
    restoreBackup(backupContent, options = {}) {
        try {
            const config = this.loadConfig();
            let data = backupContent;

            // Decrypt if needed
            if (typeof backupContent === 'string' && backupContent.startsWith('ENC:')) {
                if (!this.encryptionKey) {
                    return { success: false, error: 'Encryption key required for encrypted backup' };
                }
                data = this.decrypt(backupContent);
            }

            const backup = typeof data === 'string' ? JSON.parse(data) : data;

            if (!backup.version || !backup.collections) {
                return { success: false, error: 'Invalid backup format' };
            }

            const restored = [];
            const failed = [];

            for (const [filename, content] of Object.entries(backup.collections)) {
                const filePath = path.join(this.dataPath, filename);

                try {
                    // Create backup of current file if it exists
                    if (fs.existsSync(filePath) && options.createBackup !== false) {
                        const backupPath = filePath + '.backup';
                        fs.copyFileSync(filePath, backupPath);
                    }

                    fs.writeFileSync(filePath, JSON.stringify(content, null, 2));
                    restored.push(filename);
                } catch (e) {
                    failed.push({ file: filename, error: e.message });
                }
            }

            // Update sync status
            const status = this.loadSyncStatus();
            status.lastSync = new Date().toISOString();
            status.lastSyncStatus = 'restored';
            this.saveSyncStatus(status);

            return {
                success: true,
                restored,
                failed,
                timestamp: backup.createdAt
            };
        } catch (error) {
            return { success: false, error: error.message };
        }
    }

    // Save backup to local storage
    saveBackupLocal(backupData, filename = null) {
        const backupDir = path.join(this.dataPath, 'backups');

        if (!fs.existsSync(backupDir)) {
            fs.mkdirSync(backupDir, { recursive: true });
        }

        const backupFilename = filename || `backup_${Date.now()}.json`;
        const backupPath = path.join(backupDir, backupFilename);

        try {
            fs.writeFileSync(backupPath, backupData.data);

            // Keep only last 10 backups
            const backups = fs.readdirSync(backupDir)
                .filter(f => f.startsWith('backup_'))
                .sort()
                .reverse();

            for (let i = 10; i < backups.length; i++) {
                fs.unlinkSync(path.join(backupDir, backups[i]));
            }

            return {
                success: true,
                path: backupPath,
                filename: backupFilename
            };
        } catch (error) {
            return { success: false, error: error.message };
        }
    }

    // List local backups
    listLocalBackups() {
        const backupDir = path.join(this.dataPath, 'backups');

        if (!fs.existsSync(backupDir)) {
            return { success: true, backups: [] };
        }

        try {
            const files = fs.readdirSync(backupDir)
                .filter(f => f.endsWith('.json'))
                .map(f => {
                    const filePath = path.join(backupDir, f);
                    const stats = fs.statSync(filePath);
                    return {
                        filename: f,
                        size: stats.size,
                        created: stats.birthtime,
                        modified: stats.mtime
                    };
                })
                .sort((a, b) => new Date(b.modified) - new Date(a.modified));

            return { success: true, backups: files };
        } catch (error) {
            return { success: false, error: error.message };
        }
    }

    // Sync to Discord webhook
    async syncToDiscord(webhookUrl, options = {}) {
        const backup = this.createBackup(options);
        if (!backup.success) return backup;

        try {
            const https = require('https');
            const url = new URL(webhookUrl);

            // Create form data for file upload
            const boundary = `----WebKitFormBoundary${Date.now()}`;
            const filename = `pokemon_backup_${Date.now()}.json`;

            let body = '';
            body += `--${boundary}\r\n`;
            body += `Content-Disposition: form-data; name="file"; filename="${filename}"\r\n`;
            body += 'Content-Type: application/json\r\n\r\n';
            body += backup.data;
            body += `\r\n--${boundary}`;

            // Add message content
            body += `\r\nContent-Disposition: form-data; name="payload_json"\r\n\r\n`;
            body += JSON.stringify({
                content: `Pokemon collection backup - ${new Date().toLocaleString()}`,
                embeds: [{
                    title: 'Cloud Sync Backup',
                    description: `Backup created successfully`,
                    color: 0x3498db,
                    fields: [
                        { name: 'Files', value: backup.fileCount.toString(), inline: true },
                        { name: 'Size', value: `${Math.round(backup.size / 1024)} KB`, inline: true }
                    ],
                    timestamp: new Date().toISOString()
                }]
            });
            body += `\r\n--${boundary}--\r\n`;

            return new Promise((resolve, reject) => {
                const req = https.request({
                    hostname: url.hostname,
                    port: 443,
                    path: url.pathname + url.search,
                    method: 'POST',
                    headers: {
                        'Content-Type': `multipart/form-data; boundary=${boundary}`,
                        'Content-Length': Buffer.byteLength(body)
                    }
                }, (res) => {
                    let responseData = '';
                    res.on('data', chunk => responseData += chunk);
                    res.on('end', () => {
                        if (res.statusCode >= 200 && res.statusCode < 300) {
                            // Update sync status
                            const status = this.loadSyncStatus();
                            status.lastSync = new Date().toISOString();
                            status.lastSyncStatus = 'success';
                            this.saveSyncStatus(status);

                            resolve({
                                success: true,
                                provider: 'discord',
                                timestamp: backup.timestamp
                            });
                        } else {
                            resolve({
                                success: false,
                                error: `Discord returned status ${res.statusCode}`,
                                response: responseData
                            });
                        }
                    });
                });

                req.on('error', (e) => {
                    reject({ success: false, error: e.message });
                });

                req.write(body);
                req.end();
            });
        } catch (error) {
            return { success: false, error: error.message };
        }
    }

    // Check for changes since last sync
    getChangesSinceLastSync() {
        const status = this.loadSyncStatus();
        const lastSync = status.lastSync ? new Date(status.lastSync) : null;
        const changes = [];

        const filesToCheck = [
            'pokemon_collections.json',
            'living_dex_progress.json',
            'trade_history.json'
        ];

        for (const filename of filesToCheck) {
            const filePath = path.join(this.dataPath, filename);

            if (fs.existsSync(filePath)) {
                const stats = fs.statSync(filePath);
                const modified = new Date(stats.mtime);

                if (!lastSync || modified > lastSync) {
                    changes.push({
                        file: filename,
                        modified: stats.mtime,
                        size: stats.size
                    });
                }
            }
        }

        return {
            hasChanges: changes.length > 0,
            changes,
            lastSync: status.lastSync
        };
    }

    // Encryption helpers
    encrypt(text) {
        if (!this.encryptionKey) return text;

        try {
            const iv = crypto.randomBytes(16);
            const key = crypto.scryptSync(this.encryptionKey, 'salt', 32);
            const cipher = crypto.createCipheriv('aes-256-cbc', key, iv);
            let encrypted = cipher.update(text, 'utf8', 'hex');
            encrypted += cipher.final('hex');
            return `ENC:${iv.toString('hex')}:${encrypted}`;
        } catch (error) {
            console.error('Encryption failed:', error);
            return text;
        }
    }

    decrypt(encryptedText) {
        if (!this.encryptionKey) return encryptedText;
        if (!encryptedText.startsWith('ENC:')) return encryptedText;

        try {
            const parts = encryptedText.split(':');
            const iv = Buffer.from(parts[1], 'hex');
            const encrypted = parts[2];
            const key = crypto.scryptSync(this.encryptionKey, 'salt', 32);
            const decipher = crypto.createDecipheriv('aes-256-cbc', key, iv);
            let decrypted = decipher.update(encrypted, 'hex', 'utf8');
            decrypted += decipher.final('utf8');
            return decrypted;
        } catch (error) {
            console.error('Decryption failed:', error);
            return encryptedText;
        }
    }

    hash(text) {
        return crypto.createHash('sha256').update(text).digest('hex').substring(0, 16);
    }

    // Get sync statistics
    getStatistics() {
        const status = this.loadSyncStatus();
        const config = this.loadConfig();
        const backups = this.listLocalBackups();

        return {
            lastSync: status.lastSync,
            lastSyncStatus: status.lastSyncStatus,
            autoSync: config.autoSync,
            syncInterval: config.syncInterval,
            provider: config.provider,
            encryptionEnabled: config.encryptData,
            localBackupsCount: backups.success ? backups.backups.length : 0,
            pendingChanges: this.getChangesSinceLastSync().changes.length
        };
    }
}

module.exports = PokemonCloudSync;
