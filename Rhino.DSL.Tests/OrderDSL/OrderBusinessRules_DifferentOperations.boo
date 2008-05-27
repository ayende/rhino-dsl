when User.is_preferred and Order.total_cost > 1000:
	add_discount_precentage 5
	apply_free_shipping 
when not User.is_preferred and Order.total_cost > 1000:
	suggest_upgrade_to_preferred 
	apply_free_shipping
when User.is_not_preferred and Order.total_cost > 500:
	apply_free_shipping
