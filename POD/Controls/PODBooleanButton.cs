﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace POD.Controls
{
    public partial class PODBooleanButton : PODButton
    {
        private string _trueText;
        private string _falseText;
        private bool _state;

        public PODBooleanButton(string myFalse, string myTrue, bool myState)
        {
            

            InitializeComponent();

            //UseMnemonic = true;

            _falseText = myFalse;
            _trueText = myTrue;
            _state = myState;

            Name = myFalse.Replace("&", "");

            UpdateLabel();
        }

        public PODBooleanButton(string myFalse, string myTrue, bool myState, string tooltip, PODToolTip myTip)
        {
            //UseMnemonic = true;

            myTip.ShowAlways = true;

            _falseText = myFalse;
            _trueText = myTrue;
            _state = myState;

            Name = myFalse.Replace("&", "");

            myTip.SetToolTip(this, tooltip);

            UpdateLabel();
        }

        public string TrueLabel
        {
            set
            {
                _trueText = value;
                UpdateLabel();
            }
            get { return _trueText; }
        }

        public string FalseLabel
        {
            set 
            {
                _falseText = value;
                UpdateLabel(); 
            }
            get { return _falseText; }
        }

        public bool ButtonState
        {
            set 
            {
                _state = value;
                UpdateLabel(); 
            }
            get { return _state; }
        }

        public void FlipState()
        {
            _state = !_state;

            UpdateLabel();
        }

        public override void UpdateLabel()
        {
            if(_state == true)
            {
                Text = _trueText;
            }
            else
            {
                Text = _falseText;
            }
        }
    }
}
