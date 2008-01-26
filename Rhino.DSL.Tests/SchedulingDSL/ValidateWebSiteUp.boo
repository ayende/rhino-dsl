task "warn if website is alive":
	every 3.Minutes() 
	starting now
	when WebSite("http://example.org").IsAlive
	then:
		notify "admin@example.org", "server up!"