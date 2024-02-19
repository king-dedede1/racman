﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

using racman.Trainers;

namespace racman
{
    public partial class MainForm : Form
    {
        internal Racman state;
        public Panel TrainerPanel { get; set; }
        public Trainer Trainer { get; set; }
        private static JsonSerializerOptions jsonOptions = new JsonSerializerOptions()
        {
            WriteIndented = true
        };

        public MainForm(Racman state)
        {
            InitializeComponent();
            this.state = state;
            state.OnAPIConnect += ReloadGameSpecificStuff;
        }

        private void ReloadGameSpecificStuff()
        {
            LoadTrainer(); // this takes a long time the first time its called, but almost instant afterwards. idk why

            if (state.apiType == APIType.PS3)
            {
                pS3ToolStripMenuItem.Enabled = true;
            }
            else
            {
                pS3ToolStripMenuItem.Enabled = false;
            }
        }

        internal void LoadTrainer()
        {
            if (TrainerPanel != null)
            {
                this.Controls.Remove(TrainerPanel);
            }
            string json = File.ReadAllText(state.connected? $"{state.gameTitleID}.json":"trainer.json");
            Trainer trainer = JsonSerializer.Deserialize<Trainer>(json);
            Panel panel = new TrainerPanel(trainer);
            this.Controls.Add(panel);
            this.TrainerPanel = panel;
            this.Trainer = trainer;
        }

        internal void SaveTrainer()
        {
            if (Trainer != null)
            {
                // json pretty printing doesnt put a newline after the last brace and that really bugs me lol
                string json = JsonSerializer.Serialize(Trainer, jsonOptions);
                File.WriteAllText(state.connected ? $"{state.gameTitleID}.json" : "trainer.json", json);
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveTrainer();
            state.api?.Disconnect();
        }

        private void aboutRaCMANToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new AboutBox().ShowDialog();
        }

        private void reconnectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Hide();
            state.ShowConnectDialog();
            this.Show();
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {

        }

        private void luaConsoleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            state.consoleForm.Show();
        }
    }
}
