namespace CinemaGo.Domain.Rules
{
    public static class BookingPriceCalculator
    {
        public static decimal Calculate(bool isVip, decimal standardPrice, decimal vipPrice)
        {
            if (standardPrice < 0 || vipPrice < 0) throw new ArgumentOutOfRangeException(nameof(standardPrice), "Prices cannot be negative.");
            return isVip ? vipPrice : standardPrice;
        }
    }
}
