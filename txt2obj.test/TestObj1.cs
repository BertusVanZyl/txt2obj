using System;
using System.Collections.Generic;
using System.Text;

namespace txt2obj.test
{
    public class TestObj1
    {
        public string StringProperty { get; set; }
        public int IntegerProperty { get; set; }
        public decimal DecimalProperty { get; set; }

        public int IntegerField = 0;
        public decimal DecimalField = 0;

        public TestObj1 Complex1 { get; set; }
        public List<TestObj1> ListProperty { get; set; }
    }
}
