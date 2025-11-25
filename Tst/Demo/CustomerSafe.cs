namespace Demo
{
    // Should NOT trigger analyzer (uses preferred term Customer only)
    internal class CustomerProfile
    {
        private int customerGroup = 1000;

        public int CustomerId { get; set; }

        // property with blocked term 'Client'
        public int PreferredCustomerId { get; set; }

        public void GetCustomerValue()
        {
            var customerTemp = 0;
            _ = customerTemp + customerGroup + CustomerId;
        }
    }
}