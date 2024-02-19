﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;

namespace racman
{
    public partial class AttachGameForm : Form
    {
        public APIType apiType = APIType.None;
        public IPAddress ipAddress = null;

        public AttachGameForm()
        {
            InitializeComponent();

            comboBox1.SelectedIndex = 0;
        }

        private void continueButton_Click(object sender, EventArgs e)
        {
            this.apiType = APIType.None;
            this.ipAddress = null;

            this.Hide();
        }

        private void attachButton_Click(object sender, EventArgs e)
        {
            switch(this.comboBox1.SelectedIndex)
            {
                case 0:
                    this.apiType = APIType.PS3;
                    break;
                case 1:
                    this.apiType = APIType.RPCS3;
                    break;
                case 2:
                    this.apiType = APIType.PCSX2;
                    break;
            }

            if (this.apiType == APIType.PS3)
            {
                if (!IPAddress.TryParse(this.textBox1.Text, out ipAddress))
                {
                    MessageBox.Show("Invalid IP.");
                    return;
                }
            }

            this.Hide();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex == 0)
            {
                textBox1.Enabled = true;
            }
            else
            {
                textBox1.Enabled = false;  
            }
        }
    }

    public enum APIType
    {
        None, 
        PS3,
        RPCS3,
        PCSX2
    }
}
