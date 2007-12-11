when User.IsPreferred and Order.TotalCost > 1000:
	addDiscountPrecentage  5
	applyFreeShipping
when not User.IsPreferred and Order.TotalCost > 1000:
	suggestUpgradeToPreferred 
	applyFreeShipping
when User.IsNotPreferred and Order.TotalCost > 500:
	applyFreeShipping
