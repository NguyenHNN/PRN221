using System;
using System.Windows;

namespace EquationSolution
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ClearEquation_Click(object sender, RoutedEventArgs e)
        {
            txtA.Text = string.Empty;
            txtB.Text = string.Empty;
            txtC.Text = string.Empty;
            txtNumRoots.Text = string.Empty;
            txtX1.Text = string.Empty;
            txtX2.Text = string.Empty;
            txtResult.Text = string.Empty;
        }

        private void SolveEquation_Click(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(txtA.Text, out double a) &&
                double.TryParse(txtB.Text, out double b) &&
                double.TryParse(txtC.Text, out double c))
            {
                // check a
                if (a == 0)
                {
                    txtResult.Text = "Choose a different 0";
                    txtNumRoots.Text = "No solution";
                    txtX1.Text = string.Empty;
                    txtX2.Text = string.Empty;
                    return;
                }

                //delta=(b^2 - 4ac)
                double delta = b * b - 4 * a * c;

                if (delta > 0)
                {
                    //2 solution
                    double x1 = (-b + Math.Sqrt(delta)) / (2 * a);
                    double x2 = (-b - Math.Sqrt(delta)) / (2 * a);
                    txtNumRoots.Text = "2 solutions";
                    txtX1.Text = x1.ToString();
                    txtX2.Text = x2.ToString();
                }
                else if (delta == 0)
                {
                    //1 solution
                    double x = -b / (2 * a);
                    txtNumRoots.Text = "1 solution";
                    txtX1.Text = x.ToString();
                    txtX2.Text = string.Empty;
                }
                else
                {
                    // 0 solution
                    txtNumRoots.Text = "No solution";
                    txtX1.Text = string.Empty;
                    txtX2.Text = string.Empty;
                }
            }
            else
            {
                // Nhập sai
                txtResult.Text = "Retype a, b, c";
            }
        }
    }
}