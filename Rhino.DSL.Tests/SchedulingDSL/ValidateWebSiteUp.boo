task "warn if website is not alive":
	every 3.Minutes() 
	starting now
	when WebSite("http://example.org").IsAlive == false
	then:
		notify "admin@example.org", "server down!"