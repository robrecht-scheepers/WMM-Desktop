namespace WMM.Data
{
    public struct Balance
    {
        public double Income { get; }

        public double Expense { get; }

        public double Total => Income + Expense;

        public Balance(double income, double expense)
        {
            Income = income;
            Expense = expense;
        }
    }
}
