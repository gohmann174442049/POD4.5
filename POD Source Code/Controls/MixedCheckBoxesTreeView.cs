﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.Serialization;
using System.Runtime.InteropServices;

namespace POD.Controls
{
    public partial class MixedCheckBoxesTreeView : TreeView
    {
        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, Int32 wMsg, bool wParam, Int32 lParam);
        private const int WM_SETREDRAW = 11;
        private const int TVM_GETEXTENDEDSTYLE = 0x1100 + 45;
        private const int TVM_SETEXTENDEDSTYLE = 0x1100 + 44;
        private const int TVS_EX_DOUBLEBUFFER = 0x4;

        private int suspendCounter = 0;

        public void SuspendDrawing()
        {
            if (suspendCounter == 0)
                SendMessage(this.Handle, WM_SETREDRAW, false, 0);
            suspendCounter++;
        }

        public void ResumeDrawing()
        {
            suspendCounter--;
            if (suspendCounter == 0)
            {
                SendMessage(this.Handle, WM_SETREDRAW, true, 0);
                this.Refresh();
            }
        }

        /// <summary>
        /// Forces double buffer.
        /// </summary>
        /// <param name="e">Default event args.</param>
        protected override void OnHandleCreated(EventArgs e)
        {
            SendMessage(Handle, TVM_SETEXTENDEDSTYLE, false, TVS_EX_DOUBLEBUFFER);
            base.OnHandleCreated(e);
        }

        /// <summary>
        /// Specifies the attributes of a node
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct TV_ITEM
        {
            public int Mask;
            public IntPtr ItemHandle;
            public int State;
            public int StateMask;
            public IntPtr TextPtr;
            public int TextMax;
            public int Image;
            public int SelectedImage;
            public int Children;
            public IntPtr LParam;
        }

        public const int TVIF_STATE = 0x8;
        public const int TVIS_STATEIMAGEMASK = 0xF000;

        public const int TVM_SETITEMA = 0x110d;
        public const int TVM_SETITEM = 0x110d;
        public const int TVM_SETITEMW = 0x113f;

        public const int TVM_GETITEM = 0x110C;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(HandleRef hWnd, int msg, int wParam, ref TV_ITEM lParam);

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            
            // trap TVM_SETITEM message
            if (m.Msg == TVM_SETITEM || m.Msg == TVM_SETITEMA || m.Msg == TVM_SETITEMW)
                // check if CheckBoxes are turned on
                if (CheckBoxes)
                {
                    // get information about the node
                    TV_ITEM tv_item = (TV_ITEM)m.GetLParam(typeof(TV_ITEM));
                    HideCheckBox(tv_item);
                }
            
        }
        //used to go through the search tree and remove any duplicate checkmarks
        protected void TraverseTree(TreeNodeCollection nodes, TreeViewCancelEventArgs e)
        {
            foreach (TreeNode child in nodes)
            {
                //System.Diagnostics.Debug.WriteLine(child);
                if (child != e.Node && child.Checked == true){
                    child.Checked = false;
                }
                if (child.Nodes.Count > 0)
                {
                    TraverseTree(child.Nodes, e);
                }
            }
        }
        protected void HideCheckBox(TV_ITEM tv_item)
        {
            if (tv_item.ItemHandle != IntPtr.Zero)
            {
                // get TreeNode-object, that corresponds to TV_ITEM-object
                TreeNode currentTN = TreeNode.FromHandle(this, tv_item.ItemHandle);

                HiddenCheckBoxTreeNode hiddenCheckBoxTreeNode = currentTN as HiddenCheckBoxTreeNode;
                // check if it's HiddenCheckBoxTreeNode and
                // if its checkbox already has been hidden

                if (hiddenCheckBoxTreeNode != null)
                {
                    HandleRef treeHandleRef = new HandleRef(this, Handle);

                    // check if checkbox already has been hidden
                    TV_ITEM currentTvItem = new TV_ITEM();
                    currentTvItem.ItemHandle = tv_item.ItemHandle;
                    currentTvItem.StateMask = TVIS_STATEIMAGEMASK;
                    currentTvItem.State = 0;

                    IntPtr res = SendMessage(treeHandleRef, TVM_GETITEM, 0, ref currentTvItem);
                    bool needToHide = res.ToInt32() > 0 && currentTvItem.State != 0;

                    if (needToHide && hiddenCheckBoxTreeNode.IsHidden)
                    {
                        // specify attributes to update
                        TV_ITEM updatedTvItem = new TV_ITEM();
                        updatedTvItem.ItemHandle = tv_item.ItemHandle;
                        updatedTvItem.Mask = TVIF_STATE;
                        updatedTvItem.StateMask = TVIS_STATEIMAGEMASK;
                        updatedTvItem.State = 0;

                        // send TVM_SETITEM message
                        SendMessage(treeHandleRef, TVM_SETITEM, 0, ref updatedTvItem);
                    }
                }
            }
        }

        protected override void OnBeforeCheck(TreeViewCancelEventArgs e)
        {
            base.OnBeforeCheck(e);
            
            // prevent checking/unchecking of HiddenCheckBoxTreeNode,
            // otherwise, we will have to repeat checkbox hiding
            if (e.Node is HiddenCheckBoxTreeNode && ((HiddenCheckBoxTreeNode)e.Node).IsHidden == true)
                e.Cancel = true;
            //since parallelization does not work well with R.NET
            //automatically uncheck any boxes the user didn't just click to ensure only one box can be check at a time
            TraverseTree(base.Nodes, e);
        }
    }

    /// <summary>
    /// Represents a node with hidden checkbox
    /// </summary>
    public class HiddenCheckBoxTreeNode : TreeNode
    {
        public HiddenCheckBoxTreeNode() { }
        public HiddenCheckBoxTreeNode(string text) : base(text) { }
        public HiddenCheckBoxTreeNode(string text, TreeNode[] children) : base(text, children) {}
        public HiddenCheckBoxTreeNode(string text, int imageIndex, int selectedImageIndex) : base(text, imageIndex, selectedImageIndex) {  }
        public HiddenCheckBoxTreeNode(string text, int imageIndex, int selectedImageIndex, TreeNode[] children) : base(text, imageIndex, selectedImageIndex, children) {  }
        protected HiddenCheckBoxTreeNode(SerializationInfo serializationInfo, StreamingContext context) : base(serializationInfo, context) {  }

        public bool IsHidden = false;
    }
}
