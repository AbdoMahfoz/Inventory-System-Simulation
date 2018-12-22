using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using InventoryModels;
using InventoryTesting;

namespace InventorySimulation
{
    public partial class MainForm : Form
    {
        Panel AnimatedPanel;
        Action AnimatedPanelFinished;
        bool AnimationSlideIn;
        public MainForm()
        {
            InitializeComponent();
        }
        void ClearUI()
        {
            foreach (Control c in Controls)
            {
                if (c.GetType() == typeof(Panel))
                {
                    foreach (Control cc in ((Panel)c).Controls)
                    {
                        if (cc.GetType() == typeof(NumericUpDown))
                        {
                            ((NumericUpDown)cc).Value = 0;
                        }
                        if (cc.GetType() == typeof(DataGridView))
                        {
                            ((DataGridView)cc).Rows.Clear();
                        }
                    }
                }
            }
        }
        SimulationSystem LoadFromUI()
        {
            SimulationSystem system = new SimulationSystem()
            {
                OrderUpTo = (int)FIPOrderUpTo.Value,
                ReviewPeriod = (int)FIPReviewPeriod.Value,
                StartInventoryQuantity = (int)FIPStartingInventoryQuantity.Value,
                StartLeadDays = (int)FIPStartLeadDays.Value,
                StartOrderQuantity = (int)FIPStartOrderQuantity.Value,
                NumberOfDays = (int)FIPNumberOfDays.Value
            };
            KeyValuePair<List<Distribution>, DataGridView>[] list = new KeyValuePair<List<Distribution>, DataGridView>[]
            {
                new KeyValuePair<List<Distribution>, DataGridView>(system.DemandDistribution, SIPDemandDistribution),
                new KeyValuePair<List<Distribution>, DataGridView>(system.LeadDaysDistribution, SIPLeadDaysDistribution)
            };
            foreach (var v in list)
            {
                for (int i = 0; i < v.Value.Rows.Count - 1; i++)
                {
                    DataGridViewRow row = v.Value.Rows[i];
                    v.Key.Add(new Distribution()
                    {
                        Value = int.Parse((string)row.Cells[0].Value),
                        Probability = decimal.Parse((string)row.Cells[1].Value)
                    });
                }
            }
            return system;
        }
        void PutOnUI(SimulationSystem system)
        {
            ClearUI();
            FIPOrderUpTo.Value = system.OrderUpTo;
            FIPReviewPeriod.Value = system.ReviewPeriod;
            FIPStartingInventoryQuantity.Value = system.StartInventoryQuantity;
            FIPStartLeadDays.Value = system.StartLeadDays;
            FIPStartOrderQuantity.Value = system.StartOrderQuantity;
            FIPNumberOfDays.Value = system.NumberOfDays;
            KeyValuePair<List<Distribution>, DataGridView>[] list = new KeyValuePair<List<Distribution>, DataGridView>[]
            {
                new KeyValuePair<List<Distribution>, DataGridView>(system.DemandDistribution, SIPDemandDistribution),
                new KeyValuePair<List<Distribution>, DataGridView>(system.LeadDaysDistribution, SIPLeadDaysDistribution)
            };
            foreach (var v in list)
            {
                foreach (Distribution distribution in v.Key)
                {
                    DataGridViewRow row = new DataGridViewRow();
                    row.Cells.Add(new DataGridViewTextBoxCell(){ Value = distribution.Value.ToString() });
                    row.Cells.Add(new DataGridViewTextBoxCell(){ Value = distribution.Probability.ToString() });
                    v.Value.Rows.Add(row);
                }
            }
        }
        void FillSimulationTable(SimulationSystem system)
        {
            RSimulationTable.Rows.Clear();
            foreach (SimulationCase Case in system.SimulationTable)
            {
                DataGridViewRow row = new DataGridViewRow();
                row.Cells.Add(new DataGridViewTextBoxCell() { Value = Case.Day.ToString() });
                row.Cells.Add(new DataGridViewTextBoxCell() { Value = Case.Cycle.ToString() });
                row.Cells.Add(new DataGridViewTextBoxCell() { Value = Case.DayWithinCycle.ToString() });
                row.Cells.Add(new DataGridViewTextBoxCell() { Value = Case.BeginningInventory.ToString() });
                row.Cells.Add(new DataGridViewTextBoxCell() { Value = Case.RandomDemand.ToString() });
                row.Cells.Add(new DataGridViewTextBoxCell() { Value = Case.Demand.ToString() });
                row.Cells.Add(new DataGridViewTextBoxCell() { Value = Case.EndingInventory.ToString() });
                row.Cells.Add(new DataGridViewTextBoxCell() { Value = Case.ShortageQuantity.ToString() });
                row.Cells.Add(new DataGridViewTextBoxCell() { Value = Case.RandomLeadDays.ToString() });
                row.Cells.Add(new DataGridViewTextBoxCell() { Value = Case.LeadDays.ToString() });
                RSimulationTable.Rows.Add(row);
            }
            REndingInventoryAverage.Text = system.PerformanceMeasures.EndingInventoryAverage.ToString();
            RShortageQuantityAverage.Text = system.PerformanceMeasures.ShortageQuantityAverage.ToString();
        }
        void PanelSlide(Panel p, bool SlideIn, Action Res = null)
        {
            foreach (Control c in p.Controls)
            {
                c.Enabled = false;
            }
            AnimatedPanel = p;
            AnimationSlideIn = SlideIn;
            WPStartButton.Enabled = false;
            AnimatedPanelFinished = () =>
            {
                if (!SlideIn)
                {
                    p.Location = new Point(-p.Size.Width, 0);
                }
                else
                {
                    foreach (Control c in p.Controls)
                    {
                        c.Enabled = true;
                    }
                }
                Res?.Invoke();
            };
            AnimationTimer.Enabled = true;
        }
        private void AnimationTimer_Tick(object sender, EventArgs e)
        {
            AnimatedPanel.Location = new Point(AnimatedPanel.Location.X + 10, AnimatedPanel.Location.Y);
            if ((AnimationSlideIn && AnimatedPanel.Location.X >= 0) ||
            (!AnimationSlideIn && AnimatedPanel.Location.X >= AnimatedPanel.Size.Width))
            {
                AnimationTimer.Enabled = false;
                AnimatedPanelFinished();
            }
        }
        private void MainForm_Load(object sender, EventArgs e)
        {
            PanelSlide(WelcomePanel, true);
        }
        private void WPStartButton_Click(object sender, EventArgs e)
        {
            ClearUI();
            PanelSlide(WelcomePanel, false, () =>
            {
                PanelSlide(FirstInputPanel, true);
            });
        }
        private void FIPNextButton_Click(object sender, EventArgs e)
        {
            PanelSlide(FirstInputPanel, false, () =>
            {
                PanelSlide(SecondInputPanel, true);
            });
        }
        private void FIPBackButton_Click(object sender, EventArgs e)
        {
            PanelSlide(FirstInputPanel, false, () =>
            {
                PanelSlide(WelcomePanel, true);
            });
        }
        private void SIPBackButton_Click(object sender, EventArgs e)
        {
            PanelSlide(SecondInputPanel, false, () =>
            {
                PanelSlide(FirstInputPanel, true);
            });
        }
        private async void SIPStartSimulationButton_Click(object sender, EventArgs e)
        {
            foreach (Control c in SecondInputPanel.Controls)
            {
                c.Enabled = false;
            }
            SimulationSystem system = null;
            await Task.Run(() =>
            {
                system = LoadFromUI();
                Simulator.StartSimulation(system);
            });
            FillSimulationTable(system);
            PanelSlide(SecondInputPanel, false, () =>
            {
                PanelSlide(ResultPanel, true);
            });
        }
        private void WPLoadFromFileButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog()
            {
                InitialDirectory = Directory.GetParent(Directory.GetParent(Directory.GetCurrentDirectory().ToString()).ToString()) + "\\TestCases\\"
            };
            dialog.ShowDialog();
            PutOnUI(TestCaseManager.FromFile(dialog.FileName));
            PanelSlide(WelcomePanel, false, () =>
            {
                PanelSlide(FirstInputPanel, true);
            });
        }
        private async void WPRunAllCasesButton_Click(object sender, EventArgs e)
        {
            string res = "";
            foreach (Control c in WelcomePanel.Controls)
            {
                c.Enabled = false;
            }
            await Task.Run(() =>
            {
                res = TestCaseManager.RunAllTestCases();
            });
            MessageBox.Show(res);
            foreach (Control c in WelcomePanel.Controls)
            {
                c.Enabled = true;
            }
        }
        private async void RRunAgain_Click(object sender, EventArgs e)
        {
            SimulationSystem system = null;
            await Task.Run(() =>
            {
                system = LoadFromUI();
                Simulator.StartSimulation(system);
            });
            FillSimulationTable(system);
        }
        private void RFinish_Click(object sender, EventArgs e)
        {
            PanelSlide(ResultPanel, false, () =>
            {
                PanelSlide(WelcomePanel, true);
            });
        }
    }
}