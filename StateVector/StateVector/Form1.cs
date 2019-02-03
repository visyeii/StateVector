using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StateVector
{
    using VE = VectorEvent;

    public partial class Form1 : Form
    {

        StateVector m_stateVector;

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
        }

        private void button1_Click(object sender, EventArgs e)
        {
            m_stateVector.Refresh("a");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            m_stateVector.Refresh("b");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            m_stateVector.Refresh("c");
        }

        private void SetLog(string msg)
        {
            listBox1.Items.Add(msg);

            while (listBox1.Items.Count > 100)
            {
                listBox1.Items.RemoveAt(100);
            }
        }
    }
}
