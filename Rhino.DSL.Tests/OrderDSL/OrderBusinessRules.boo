when User.IsPreferred and Order.TotalCost > 1000:
	AddDiscountPrecentage  5
	ApplyFreeShipping
when not User.IsPreferred and Order.TotalCost > 1000:
	SuggestUpgradeToPreferred 
	ApplyFreeShipping
when User.IsNotPreferred and Order.TotalCost > 500:
	ApplyFreeShipping
