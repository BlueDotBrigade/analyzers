namespace Demo
{
    // Should trigger analyzer (class name contains blocked term 'Cust')
    internal class Cust
    {
        // field with blocked term 'client'
        private int clientGroup = 1000;

        // property with blocked term 'Client'
        public int ClientId { get; set; }

        // property with blocked term 'Client'
        public int PreferredClientId { get; set; }

        // method with blocked term 'Cust'
        public void GetCustValue()
        {
            // local variable with blocked term 'client'
            var clientTemp = 0;
            _ = clientTemp + clientGroup + ClientId;
        }
    }
}