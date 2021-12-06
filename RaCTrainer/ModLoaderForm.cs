﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NLua;

namespace racman
{
    public partial class ModLoaderForm : Form
    {
        public static Mod[] mods;
        string gameModFolder;

        bool reloading = false;

        public ModLoaderForm()
        {
            InitializeComponent();

            gameModFolder = $"{Directory.GetCurrentDirectory()}\\mods\\{AttachPS3Form.game}\\";

            if (mods == null)
            {
                mods = this.LoadMods().ToArray();
            }

            foreach (var mod in mods)
            {
                this.modsCheckedListBox.Items.Add(mod.name, mod.loaded);
            }
        }

        public List<Mod> LoadMods()
        {
            if (!Directory.Exists(gameModFolder))
            {
                return new List<Mod>();
            }

            var modFolders = Directory.EnumerateDirectories(gameModFolder);

            List<Mod> mods = new List<Mod>();

            foreach (var modFolder in modFolders)
            {
                Mod mod = GetMod(modFolder);

                if (mod != null)
                {
                    mods.Add(mod);
                }
            }

            return mods;
        }

        private void ReloadMods()
        {
            reloading = true;

            List<Mod> allMods = new List<Mod>(ModLoaderForm.mods);
            List<string> modsFolders = new List<string>();

            foreach(Mod mod in mods)
            {
                modsFolders.Add(mod.modFolder);

                // Don't reload mods that are currently loaded
                if (!mod.loaded)
                {
                    var index = allMods.FindIndex(x => x.modFolder == mod.modFolder);

                    // Remove mod from list if it's removed from file system
                    if (!Directory.Exists(mod.modFolder))
                    {
                        allMods.RemoveAt(index);
                        continue;
                    }

                    allMods.RemoveAt(index);
                    allMods.Insert(index, GetMod(mod.modFolder));
                }
            }

            var modFolders = Directory.EnumerateDirectories(gameModFolder);

            // Load new mods
            foreach (var modFolder in modFolders)
            {
                // Ignore already loaded mods
                if (modsFolders.Contains(modFolder))
                {
                    continue;
                }

                Mod mod = GetMod(modFolder);

                if (mod != null)
                {
                    allMods.Add(mod);
                }
            }

            mods = allMods.ToArray();

            this.modsCheckedListBox.Items.Clear();
            foreach (var mod in mods)
            {
                this.modsCheckedListBox.Items.Add(mod.name, mod.loaded);
            }

            reloading = false;
        }

        private Mod GetMod(string modFolder)
        {
            if (!File.Exists($"{modFolder}\\patch.txt"))
            {
                return null;
            }

            var mod = new Mod();
            mod.modFolder = modFolder;

            var patchFileStream = File.OpenRead($"{modFolder}\\patch.txt");

            using (StreamReader reader = new StreamReader(patchFileStream))
            {
                mod.patchLines = reader.ReadToEnd().Split('\n').ToList();

                foreach (var patchLine in mod.patchLines)
                {
                    if (patchLine.Length > 2 && patchLine.Substring(0, 2) == "#-")
                    {
                        var patchLineComponents = patchLine.Split(new char[] { ':' }, 2);
                        if (patchLineComponents.Length < 2)
                        {
                            continue;
                        }

                        var key = patchLineComponents[0].Substring(2).Trim();
                        var value = patchLineComponents[1].Trim();

                        mod.variables[key] = value;
                    }
                }
            }

            if (mod.variables.ContainsKey("name"))
            {
                mod.name = mod.variables["name"];
            }
            else
            {
                mod.name = new DirectoryInfo(modFolder).Name;
            }

            return mod;
        }

        private void modsCheckedListBox_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (reloading)
            {
                return;
            }

            if (e.NewValue == CheckState.Checked)
            {
                if (!ModLoaderForm.mods[e.Index].Load())
                {
                    e.NewValue = CheckState.Unchecked;
                }
            }
            else
            {
                ModLoaderForm.mods[e.Index].Unload();
            }
        }

        private void ModLoaderForm_Load(object sender, EventArgs e)
        {

        }

        private void modsCheckedListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (modsCheckedListBox.SelectedIndex < 0)
            {
                return;
            }

            Mod mod = ModLoaderForm.mods[modsCheckedListBox.SelectedIndex];

            modNameLabel.Text = mod.name;
            authorNameLabel.Text = "N/A";
            versionLabel.Text = "N/A";
            linkLabel.Text = "";
            descriptionTextBox.Text = "";
            
            if (mod.variables.ContainsKey("author"))
            {
                authorNameLabel.Text = mod.variables["author"];
            }

            if (mod.variables.ContainsKey("version"))
            {
                versionLabel.Text = mod.variables["version"];
            }

            if (mod.variables.ContainsKey("href") && (mod.variables["href"].StartsWith("https://") || mod.variables["href"].StartsWith("http://")))
            {
                linkLabel.Text = mod.variables["href"];
                linkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler((s, ev) => System.Diagnostics.Process.Start(mod.variables["href"]));
            }

            if (mod.variables.ContainsKey("description"))
            {
                descriptionTextBox.Text = mod.variables["description"];
            }
        }

        private void addZipButton_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "ZIP file (*.zip)|*.zip";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        ZipFile.ExtractToDirectory(openFileDialog.FileName, $"{Directory.GetCurrentDirectory()}\\mods\\{AttachPS3Form.game}\\");
                    } catch (IOException exception)
                    {
                        // There's apparently no easy way to tell ZipFile.ExtractToDirectory to overwrite files smh
                        MessageBox.Show("Failed to extract mod from ZIP, maybe a mod with the same name is already installed?");
                    }

                    this.ReloadMods();
                }
            }
        }

        private void ModLoaderForm_Activated(object sender, EventArgs e)
        {
            this.ReloadMods();
        }

        private void openConsoleButton_Click(object sender, EventArgs e)
        {
            if (!AttachPS3Form.console.IsDisposed)
            {
                AttachPS3Form.console.Show();
            } else
            {
                RacManConsole console = new RacManConsole();
                console.Show();
            }
        }
    }

    public class Mod
    {
        public string name;
        public string version;

        public bool loaded = false;

        public string modFolder = "";

        public Dictionary<string, string> variables = new Dictionary<string, string>();
        public List<string> patchLines = new List<string>();
        public Dictionary<uint, byte[]> originalData = new Dictionary<uint, byte[]>();

        List<LuaAutomation> luaAutomations = new List<LuaAutomation>();

        private void LoadOriginalData()
        {
            foreach (string patch in patchLines)
            {
                if (patch.Length < 2 || patch[0] == '#')
                {
                    continue;
                }

                var patchComponents = patch.Split(':');
                var addressString = patchComponents[0].Trim();
                
                if(!addressString.StartsWith("0x"))
                {
                    continue;  // Probably Lua automation or something
                }

                uint address = UInt32.Parse(addressString.Substring(addressString.IndexOf("0x") + 2), System.Globalization.NumberStyles.HexNumber);
                var value = patchComponents[1].Trim();

                byte[] patchBytes;

                if (value.Contains("0x"))
                {
                    patchBytes = BitConverter.GetBytes(UInt32.Parse(value.Substring(value.IndexOf("0x") + 2), System.Globalization.NumberStyles.HexNumber)).Reverse().ToArray();
                }
                else
                {
                    patchBytes = File.ReadAllBytes($"{modFolder}\\{value}");
                }

                int bytesRead = 0;
                byte[] bytesToWrite = new byte[] { };
                List<byte> ogData = new List<byte>();
                while (bytesRead < patchBytes.Length)
                {
                    bytesToWrite = patchBytes.Skip(bytesRead).Take(1024).ToArray();

                    Ratchetron api = (Ratchetron)func.api;
                    ogData.AddRange(api.ReadMemory(AttachPS3Form.pid, address + (uint)bytesRead, (uint)bytesToWrite.Length));

                    bytesRead += bytesToWrite.Length;
                }

                originalData[address] = ogData.ToArray();
            }
        }

        public bool Load()
        {
            Console.WriteLine($"Loading mod: {this.name}");

            if (this.originalData.Keys.Count <= 0)
            {
                this.LoadOriginalData();
            }

            Ratchetron api = (Ratchetron)func.api;

            bool dirty = false;

            foreach (string patch in patchLines)
            {
                if (patch.Length < 2 || patch[0] == '#')
                {
                    continue;
                }

                var patchComponents = patch.Split(':');
                var addressString = patchComponents[0].Trim();
                var value = patchComponents[1].Trim();

                if (addressString == "automation")
                {
                    // Lua "automation" file

                    if (!this.LoadLuaAutomation($"{modFolder}\\{value}"))
                    {
                        // We need to unload, but we need to do it later because we don't know what patches might have been applied that need to be reverted.
                        dirty = true;
                    }

                    continue;
                }

                uint address = UInt32.Parse(addressString.Substring(addressString.IndexOf("0x") + 2), System.Globalization.NumberStyles.HexNumber);

                byte[] patchBytes;

                if (value.Contains("0x"))
                {
                    patchBytes = BitConverter.GetBytes(UInt32.Parse(value.Substring(value.IndexOf("0x") + 2), System.Globalization.NumberStyles.HexNumber)).Reverse().ToArray();
                }
                else
                {
                    patchBytes = File.ReadAllBytes($"{modFolder}\\{value}");
                }

                int bytesWritten = 0;
                byte[] bytesToWrite = new byte[] { };
                while (bytesWritten < patchBytes.Length)
                {
                    bytesToWrite = patchBytes.Skip(bytesWritten).Take(1024).ToArray();

                    api.WriteMemory(AttachPS3Form.pid, address + (uint)bytesWritten, (uint)bytesToWrite.Length, bytesToWrite);

                    bytesWritten += bytesToWrite.Length;
                }
            }

            if (dirty)
            {
                // We failed something at some point, revert patches.
                this.Unload();
                return false;
            }

            this.loaded = true;
            return true;
        }

        private bool LoadLuaAutomation(string filename)
        {
            LuaAutomation automation = new LuaAutomation(filename, AttachPS3Form.game, this);
            this.luaAutomations.Add(automation);

            return !automation.failed;
        }

        public void Unload()
        {
            foreach (KeyValuePair<uint, byte[]> entry in this.originalData)
            {
                int bytesWritten = 0;
                byte[] bytesToWrite = new byte[] { };
                while (bytesWritten < entry.Value.Length)
                {
                    bytesToWrite = entry.Value.Skip(bytesWritten).Take(1024).ToArray();

                    Ratchetron api = (Ratchetron)func.api;
                    api.WriteMemory(AttachPS3Form.pid, entry.Key + (uint)bytesWritten, (uint)bytesToWrite.Length, bytesToWrite);

                    bytesWritten += bytesToWrite.Length;
                }
            }

            // Stop and clear out Lua automations
            foreach(LuaAutomation automation in luaAutomations)
            {
                automation.Unload();
            }

            this.luaAutomations.Clear();

            this.loaded = false;
        }
    }

}
