
window.ControlsUi = (function() {

	var SUPPORT_TRANSFERABLE,
		_ControlsUi = ControlsUi, // used for noConflict method
		ControlsUi = {};       // object assigned to window.Vehicle
	
	// TODO: Put this in another JS - soon
    var ControlsText = function() {
        this.camName = 'CameraName';
        this.enableOculus = false;
        this.speed     = 0.1;
        this.loading   = 0.0;
        this.camX      = 0.0;
        this.camY      = 0.9;
        this.camZ      = 1.5;
        this.enableLights = function() {
            if(dirLight.visible === true) {
                var dark_color = new THREE.Color(0x000000);
                dirLight.visible = false;
                hemiLight.visible = false;
                uniforms.topColor.value.copy( dark_color );
                uniforms.bottomColor.value.copy( dark_color );
                scene.fog.color.copy( uniforms.bottomColor.value );
            } 
            else { 
                dirLight.visible =true;
                hemiLight.visible = true;
                uniforms.topColor.value.copy( hemiLight.color );
                uniforms.bottomColor.value.copy( new THREE.Color(0xffffff) );
                scene.fog.color.copy( uniforms.bottomColor.value );
            } 
        };
      // Define render logic ...
    };	
		
	ControlsUi.init = function()
	{
        var ctrltext = new ControlsText();
        var gui = new dat.GUI();
        
        var f1 = gui.addFolder('General');
        var occChange = f1.add(ctrltext, 'enableOculus', false).listen();
        occChange.onChange(function(value) { window.onWindowResize(); } );
        
        f1.add(ctrltext, 'speed', -1.0, 20.0).listen();
        f1.add(ctrltext, 'enableLights');
        f1.add(ctrltext, 'loading', 0.0, 100.0).listen();
        f1.open();
        
        var f2 = gui.addFolder('Camera');
        var camChange = f2.add(ctrltext, 'camName', Cameras.all_cameras_names);

        camChange.onChange(function(value) {
          // Fires when a controller loses focus.
          Cameras.changeCamera(value);
          ctrltext.camX = Cameras.curr_camera.position.x;
          ctrltext.camY = Cameras.curr_camera.position.y;
          ctrltext.camZ = Cameras.curr_camera.position.z;
          //alert("The new value is " + value);
        });
    
        f2.add(ctrltext, 'camX', -15.0, 15.0).listen();
        f2.add(ctrltext, 'camY', -15.0, 15.0).listen();
        f2.add(ctrltext, 'camZ', -15.0, 15.0).listen();
				
        return ctrltext;
	};
		
	return ControlsUi;
})();