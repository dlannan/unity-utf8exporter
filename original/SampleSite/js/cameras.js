
window.Cameras = (function() {

	var SUPPORT_TRANSFERABLE,
		_Cameras = Cameras, // used for noConflict method
		Cameras = {},       // object assigned to window.Vehicle
		curr_camera,
		all_cameras = {},   // All the cameras that have been made
		all_cameras_names = [],
		all_cameras_count = 0;
		
	Cameras.init = function()
	{
	    Cameras.all_cameras = {};
	    Cameras.all_cameras_names = [];
	    Cameras.all_cameras_count = 0;
	};
	
	Cameras.newCamera = function( cname, isOrtho )
	{
	    var new_camera, cam_name;
	    if(cname) {
	        cam_name = cname; 
	    }
	    else {
	        cam_name = "Camera" + Cameras.all_cameras_count; 
	    }
	    
	    if(isOrtho)
	    {
	        var height = window.innerHeight;
	        var width = window.innerWidth;
            new_camera = new THREE.OrthographicCamera( width / - 2, width / 2, height / 2, height / - 2, 0.5, 5000 );
	    }
	    else
		{
		    new_camera = new THREE.PerspectiveCamera( 45, window.innerWidth / window.innerHeight, 0.5, 5000	);
	    }
	    
	    new_camera.name = cam_name;
		Cameras.all_cameras[cam_name] = new_camera;
		Cameras.all_cameras_names.push( cam_name );
		Cameras.all_cameras_count++;
		Cameras.curr_camera = new_camera;
		return new_camera;
	};
		
	Cameras.changeCamera = function( cname )
	{
	    var cam = Cameras.all_cameras[cname];
	    if(cam)
	        Cameras.curr_camera = cam;
	    camera = Cameras.curr_camera;
	};
	
	return Cameras;
})();