using System;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace ScientificCalculator
{
    public class CalculatorForm : Form
    {
        private TextBox txtDisplay;
        private TableLayoutPanel grid;

        // State variables
        private bool isDegrees = true;
        private bool isInverse = false;
        private double lastAnswer = 0;

        // Colors based on the image
        private Color bgColor = Color.FromArgb(30, 30, 30);
        private Color btnDark = Color.FromArgb(44, 44, 46); // Scientific buttons
        private Color btnLight = Color.FromArgb(51, 51, 51); // Number buttons
        private Color btnBlue = Color.FromArgb(10, 132, 255); // Equals button
        private Color txtColor = Color.White;

        public CalculatorForm()
        {
            // Form Setup
            this.Text = "Scientific Calculator";
            this.Size = new Size(520, 480);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = bgColor;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            // Main Grid Setup
            grid = new TableLayoutPanel();
            grid.Dock = DockStyle.Fill;
            grid.ColumnCount = 7;
            grid.RowCount = 6;
            grid.BackColor = bgColor;
            grid.Padding = new Padding(10);

            // Columns (7 equal columns)
            for (int i = 0; i < 7; i++)
                grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 14.28F));

            // Rows (1 large for display, 5 for buttons)
            grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 90F));
            for (int i = 0; i < 5; i++)
                grid.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));

            this.Controls.Add(grid);

            // Setup Display
            txtDisplay = new TextBox();
            txtDisplay.Dock = DockStyle.Fill;
            txtDisplay.Font = new Font("Segoe UI", 28, FontStyle.Regular);
            txtDisplay.BackColor = bgColor;
            txtDisplay.ForeColor = txtColor;
            txtDisplay.BorderStyle = BorderStyle.None;
            txtDisplay.TextAlign = HorizontalAlignment.Right;
            txtDisplay.ReadOnly = true;
            txtDisplay.Text = "0";
            txtDisplay.Margin = new Padding(5, 20, 5, 20);
            grid.Controls.Add(txtDisplay, 0, 0);
            grid.SetColumnSpan(txtDisplay, 7);

            // --- Build the Button Layout ---

            // Row 1
            CreateButton("Deg | Rad", 0, 1, ToggleDegRad_Click, btnDark, 2);
            CreateButton("x!", 2, 1, Func_Click, btnDark);
            CreateButton("(", 3, 1, Input_Click, btnLight);
            CreateButton(")", 4, 1, Input_Click, btnLight);
            CreateButton("%", 5, 1, Input_Click, btnLight);
            CreateButton("AC", 6, 1, Clear_Click, btnLight);

            // Row 2
            CreateButton("Inv", 0, 2, ToggleInv_Click, btnDark);
            CreateButton("sin", 1, 2, Func_Click, btnDark);
            CreateButton("ln", 2, 2, Func_Click, btnDark);
            CreateButton("7", 3, 2, Input_Click, btnLight);
            CreateButton("8", 4, 2, Input_Click, btnLight);
            CreateButton("9", 5, 2, Input_Click, btnLight);
            CreateButton("÷", 6, 2, Input_Click, btnLight);

            // Row 3
            CreateButton("π", 0, 3, Func_Click, btnDark);
            CreateButton("cos", 1, 3, Func_Click, btnDark);
            CreateButton("log", 2, 3, Func_Click, btnDark);
            CreateButton("4", 3, 3, Input_Click, btnLight);
            CreateButton("5", 4, 3, Input_Click, btnLight);
            CreateButton("6", 5, 3, Input_Click, btnLight);
            CreateButton("×", 6, 3, Input_Click, btnLight);

            // Row 4
            CreateButton("e", 0, 4, Func_Click, btnDark);
            CreateButton("tan", 1, 4, Func_Click, btnDark);
            CreateButton("√", 2, 4, Func_Click, btnDark);
            CreateButton("1", 3, 4, Input_Click, btnLight);
            CreateButton("2", 4, 4, Input_Click, btnLight);
            CreateButton("3", 5, 4, Input_Click, btnLight);
            CreateButton("−", 6, 4, Input_Click, btnLight);

            // Row 5
            CreateButton("Ans", 0, 5, Func_Click, btnDark);
            CreateButton("EXP", 1, 5, Func_Click, btnDark);
            CreateButton("x^y", 2, 5, Input_Click, btnDark);
            CreateButton("0", 3, 5, Input_Click, btnLight);
            CreateButton(".", 4, 5, Input_Click, btnLight);
            CreateButton("=", 5, 5, Equals_Click, btnBlue);
            CreateButton("+", 6, 5, Input_Click, btnLight);
        }

        // Helper to create buttons
        private void CreateButton(string text, int col, int row, EventHandler handler, Color bg, int colSpan = 1)
        {
            Button btn = new Button();
            btn.Text = text;
            btn.Dock = DockStyle.Fill;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.Font = new Font("Segoe UI", 12, FontStyle.Regular);
            btn.BackColor = bg;
            btn.ForeColor = txtColor;
            btn.Margin = new Padding(3);
            btn.Cursor = Cursors.Hand;
            btn.Click += handler;
            btn.Tag = text; // Store original text for logic

            grid.Controls.Add(btn, col, row);
            if (colSpan > 1) grid.SetColumnSpan(btn, colSpan);
        }

        // --- Event Handlers ---

        private void Input_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            string input = btn.Tag.ToString();

            if (txtDisplay.Text == "0" || txtDisplay.Text == "Error")
                txtDisplay.Text = "";

            // Replace visual math symbols with standard programming symbols
            if (input == "×") input = "*";
            if (input == "÷") input = "/";
            if (input == "−") input = "-";

            // FIX: Convert x^y button to the actual power symbol
            if (input == "x^y") input = "^";

            txtDisplay.Text += input;
        }

        private void Func_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            string func = btn.Tag.ToString();

            if (txtDisplay.Text == "0" || txtDisplay.Text == "Error")
                txtDisplay.Text = "";

            // Handle Inverse toggles
            if (isInverse)
            {
                if (func == "sin") func = "asin(";
                else if (func == "cos") func = "acos(";
                else if (func == "tan") func = "atan(";
                else if (func == "ln") func = "e^";
                else if (func == "log") func = "10^";
                else if (func == "√") func = "^2";
                else if (func == "sin(") func = "asin(";
                else if (func == "cos(") func = "acos(";
                else if (func == "tan(") func = "atan(";
            }
            else
            {
                if (func == "sin") func = "sin(";
                else if (func == "cos") func = "cos(";
                else if (func == "tan") func = "tan(";
                else if (func == "ln") func = "ln(";
                else if (func == "log") func = "log(";
                else if (func == "√") func = "√(";
            }

            if (func == "π") txtDisplay.Text += "π";
            else if (func == "e") txtDisplay.Text += "e";
            else if (func == "Ans") txtDisplay.Text += lastAnswer.ToString(CultureInfo.InvariantCulture);
            else if (func == "EXP") txtDisplay.Text += "E";
            else txtDisplay.Text += func;
        }

        private void Clear_Click(object sender, EventArgs e)
        {
            txtDisplay.Text = "0";
        }

        private void ToggleDegRad_Click(object sender, EventArgs e)
        {
            isDegrees = !isDegrees;
            Button btn = (Button)sender;
            btn.Text = isDegrees ? "Deg | Rad" : "Rad | Deg";
            btn.BackColor = isDegrees ? btnDark : btnBlue;
        }

        private void ToggleInv_Click(object sender, EventArgs e)
        {
            isInverse = !isInverse;
            Button btn = (Button)sender;
            btn.BackColor = isInverse ? btnBlue : btnDark;

            // Update button labels
            foreach (Control c in grid.Controls)
            {
                if (c is Button b)
                {
                    if (b.Tag.ToString() == "sin") b.Text = isInverse ? "sin⁻¹" : "sin";
                    if (b.Tag.ToString() == "cos") b.Text = isInverse ? "cos⁻¹" : "cos";
                    if (b.Tag.ToString() == "tan") b.Text = isInverse ? "tan⁻¹" : "tan";
                    if (b.Tag.ToString() == "ln") b.Text = isInverse ? "eˣ" : "ln";
                    if (b.Tag.ToString() == "log") b.Text = isInverse ? "10ˣ" : "log";
                    if (b.Tag.ToString() == "√") b.Text = isInverse ? "x²" : "√";
                }
            }
        }

        private void Equals_Click(object sender, EventArgs e)
        {
            try
            {
                string resultStr = EvaluateMath(txtDisplay.Text);
                double result = double.Parse(resultStr, CultureInfo.InvariantCulture);

                // Round to 10 decimal places to prevent ugly floating point errors
                result = Math.Round(result, 10);

                // Clean up trailing zeros for integers
                if (result % 1 == 0) txtDisplay.Text = ((long)result).ToString();
                else txtDisplay.Text = result.ToString();

                lastAnswer = result;
            }
            catch
            {
                txtDisplay.Text = "Error";
            }
        }

        // --- THE MATH PARSER ---
        private string EvaluateMath(string expr)
        {
            // 1. Replace symbols and constants
            expr = expr.Replace("×", "*").Replace("÷", "/").Replace("−", "-");
            expr = expr.Replace("π", Math.PI.ToString(CultureInfo.InvariantCulture));
            expr = expr.Replace("e", Math.E.ToString(CultureInfo.InvariantCulture));
            expr = expr.Replace("E", "*10^"); // Handle EXP notation
            expr = expr.Replace("%", "/100.0");

            // 2. Handle Factorials (e.g. 5!)
            while (expr.Contains("!"))
            {
                Match m = Regex.Match(expr, @"([0-9.]+)!");
                if (!m.Success) break;
                double val = double.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture);
                if (val < 0 || val % 1 != 0) throw new Exception("Invalid Factorial");
                double fact = 1;
                for (int i = 1; i <= val; i++) fact *= i;
                expr = expr.Replace(m.Value, fact.ToString(CultureInfo.InvariantCulture));
            }

            // 3. Handle Powers (e.g. 2^3)
            // FIX: Added [-?] to the exponent so it handles things like 5^-2
            while (expr.Contains("^"))
            {
                Match m = Regex.Match(expr, @"([0-9.]+)\^([-]?[0-9.]+)");
                if (!m.Success) break;
                double baseNum = double.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture);
                double exp = double.Parse(m.Groups[2].Value, CultureInfo.InvariantCulture);
                expr = expr.Replace(m.Value, Math.Pow(baseNum, exp).ToString(CultureInfo.InvariantCulture));
            }

            // 4. Handle Functions (sin, cos, tan, ln, log, sqrt)
            // Finds innermost parentheses first, evaluates, then replaces.
            string pattern = @"(sin|cos|tan|asin|acos|atan|ln|log|√)\(([^()]+)\)";
            while (Regex.IsMatch(expr, pattern))
            {
                expr = Regex.Replace(expr, pattern, m =>
                {
                    string func = m.Groups[1].Value;
                    double val = double.Parse(EvaluateMath(m.Groups[2].Value), CultureInfo.InvariantCulture);

                    switch (func)
                    {
                        case "sin": return isDegrees ? Math.Sin(val * Math.PI / 180).ToString(CultureInfo.InvariantCulture) : Math.Sin(val).ToString(CultureInfo.InvariantCulture);
                        case "cos": return isDegrees ? Math.Cos(val * Math.PI / 180).ToString(CultureInfo.InvariantCulture) : Math.Cos(val).ToString(CultureInfo.InvariantCulture);
                        case "tan": return isDegrees ? Math.Tan(val * Math.PI / 180).ToString(CultureInfo.InvariantCulture) : Math.Tan(val).ToString(CultureInfo.InvariantCulture);
                        case "asin": return isDegrees ? (Math.Asin(val) * 180 / Math.PI).ToString(CultureInfo.InvariantCulture) : Math.Asin(val).ToString(CultureInfo.InvariantCulture);
                        case "acos": return isDegrees ? (Math.Acos(val) * 180 / Math.PI).ToString(CultureInfo.InvariantCulture) : Math.Acos(val).ToString(CultureInfo.InvariantCulture);
                        case "atan": return isDegrees ? (Math.Atan(val) * 180 / Math.PI).ToString(CultureInfo.InvariantCulture) : Math.Atan(val).ToString(CultureInfo.InvariantCulture);
                        case "ln": return Math.Log(val).ToString(CultureInfo.InvariantCulture);
                        case "log": return Math.Log10(val).ToString(CultureInfo.InvariantCulture);
                        case "√": return Math.Sqrt(val).ToString(CultureInfo.InvariantCulture);
                        default: return "0";
                    }
                });
            }

            // 5. Final basic arithmetic evaluation using DataTable
            var result = new DataTable().Compute(expr, null);
            return result.ToString();
        }
    }

    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new CalculatorForm());
        }
    }
}