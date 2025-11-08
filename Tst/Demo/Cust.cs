namespace Demo
{
    // Should trigger analyzer (class name contains blocked term 'Cust')
    internal class Cust
    {
        // field with blocked term 'client'
        private int clientValue;

        // property with blocked term 'Client'
        public int ClientValue { get; set; }

        // method with blocked term 'Cust'
        public void GetCustValue()
        {
            // local variable with blocked term 'client'
            var clientCount = 0;
            _ = clientCount + clientValue + ClientValue;
        }
    }
}
