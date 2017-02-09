"use strict";

class CursorSyncer
{
	constructor(inRippleLink, syncFrequency)
	{
		// The ripple link we should send the cursor updates down
		this.rippleLink = inRippleLink;
		// The target frequency in fps at we should send sursor updates.
		this.cursorUpdateFrequency = syncFrequency;
		
		this.otherClients = [];
		
		// Register ourselves to start sending cursor updates once the ripple
		// link connects
		this.rippleLink.on("connect", this.setup.bind(this));
	}
	
	setup()
	{
		// The last time we sent a cursor update to the server.
		this.lastCursorUpdate = 0;
		
		document.addEventListener("mousemove", (function(event) {
			this.cursorPosition = {
				X: event.clientX,
				Y: event.clientY
			};
			
			setTimeout((function() {
			    // Throttle the cursor updates we send to the server - a high
			    // update frequency here will just consume bandwidth and is only
			    // noticable if you have a good connection
				if(+new Date() - this.lastCursorUpdate < 1 / this.cursorUpdateFrequency)
					return false;
					
				// Update the server on the mouse's position
				this.sendCursorUpdate();
				
				this.lastFrameStart = +new Date();
			}).bind(this), 1 / this.cursorUpdateFrequency);
			
		}).bind(this));
		
		this.sendCursorUpdate();
	}
	
	sendCursorUpdate()
	{
		// Update the server on the mouse's position
		this.rippleLink.send({
			"event": "CursorPosition",
			"AbsCursorPosition": this.cursorPosition
		});
	}
}

export default CursorSyncer;
