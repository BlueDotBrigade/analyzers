namespace Demo
{
    // Should NOT trigger analyzer (uses preferred term Customer only)
    internal class CustomerProfile
    {
        private int customerValue;

        public int CustomerValue { get; set; }
        public void GetCustomerValue()
        {
            var customerCount = 0;
            _ = customerCount + customerValue + CustomerValue;
        }
    }
}
