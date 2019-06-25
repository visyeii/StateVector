﻿using System;
using System.Windows.Forms;

namespace StateVector
{
    using VE = VectorEvent;

    public partial class Form1 : Form
    {

        StateVector m_stateVector;
        Func<StateVectorTraceInfo, Exception> m_TraceFunc_DefaultBackup = null;

        public Form1()
        {
            InitializeComponent();
        }

        private void InitState()
        {
            SetLog("start");
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            VE[] list = {
                new VE("init",              VE.TailOr("a", "b", "c"),   InitState, () => { SetLog("!"); }),
                new VE("a",                 "b",                        () => { SetLog("a->b"); }),
                new VE("b",                 "a",                        () => { SetLog("b->a"); }),
                new VE("a",                 "a",                        () => { SetLog("a->a"); }),
                new VE("b",                 "b",                        () => { SetLog("b->b"); }),
                new VE(VE.HeadOr("a", "b"), "c",                        () => { SetLog("a|b->c"); }),
                new VE("c",                 VE.TailOr("a", "b"),        () => { SetLog("c->a|b"); })
            };

            m_stateVector = new StateVector("init", list);
            m_stateVector.EnableRefreshTrace = true;
            m_TraceFunc_DefaultBackup = m_stateVector.TraceFunc;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                m_stateVector.Refresh("a");
            }
            catch (NotImplementedException ex)
            {
                SetLog("NotImplementedException:" + ex.Message);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                m_stateVector.Refresh("b");
            }
            catch (NotImplementedException ex)
            {
                SetLog("NotImplementedException:" + ex.Message);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                m_stateVector.Refresh("c");
            }
            catch (NotImplementedException ex)
            {
                SetLog("NotImplementedException:" + ex.Message);
            }
        }

        private void SetLog(string msg)
        {
            listBox1.Items.Add(msg);

            while (listBox1.Items.Count > 100)
            {
                listBox1.Items.RemoveAt(100);
            }
        }

        private void RadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (((RadioButton)sender).Checked)
            {
                if (ReferenceEquals(sender, radioButton1))
                {
                    m_stateVector.EnableRefreshTrace = true;
                    m_stateVector.TraceFunc = m_TraceFunc_DefaultBackup;
                }
                else if (ReferenceEquals(sender, radioButton2))
                {
                    m_stateVector.EnableRefreshTrace = false;
                    //m_stateVector.TraceFunc = null;
                }
                else if (ReferenceEquals(sender, radioButton3))
                {
                    m_stateVector.EnableRefreshTrace = true;
                    m_stateVector.TraceFunc = new Func<StateVectorTraceInfo, Exception>(
                    (StateVectorTraceInfo traceInfo) => {// example
                        Exception ex = null;

                        string msg = $"{traceInfo.ListName} {traceInfo.Tag} {traceInfo.Head} -> {traceInfo.Tail} "
                                    + $"do[{traceInfo.Index}].priority({traceInfo.Priority}) "
                                    + $"{(traceInfo.FuncInfo == null ? "" : traceInfo.FuncInfo.Name)}";

                        if (traceInfo.IsHit)
                        {
                            if (traceInfo.IsDone)
                            {
                                SetLog(" done.");
                            }
                            else
                            {
                                SetLog(msg);
                            }
                        }
                        else
                        {
                            SetLog("Not Hit Rule:" + msg);
                        }

                        return ex;
                    });
                }
            }
        }
    }
}
