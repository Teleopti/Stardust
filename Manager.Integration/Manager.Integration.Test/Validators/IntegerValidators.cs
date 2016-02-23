namespace Manager.Integration.Test.Validators
{
    public static class IntegerValidators
    {
        public static bool Value1IsLargerThenOrEqualToValue2Validator(int value1,
                                                                      int value2)
        {
            return value1 >= value2;
        }
    

        public static bool Value1IsEqualToValue2Validator(int value1,
                                                          int value2)
        {
            return value1 == value2;
        }
    }
}