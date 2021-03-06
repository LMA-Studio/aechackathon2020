import * as THREE from 'https://cdn.jsdelivr.net/npm/three@0.118.3/build/three.module.js';
import { OrbitControls } from 'https://cdn.jsdelivr.net/npm/three@0.118.3/examples/jsm/controls/OrbitControls.js';
import { OBJLoader } from 'https://cdn.jsdelivr.net/npm/three@0.118.3/examples/jsm/loaders/OBJLoader.js';

function loadModel(modelName)
{
  const clock = new THREE.Clock();
  
  const container = document.getElementById(`viewport_${modelName}`);
  
  const scene = new THREE.Scene();
  scene.background = new THREE.Color( 0xa0a0a0 );
  scene.fog = new THREE.Fog( 0xa0a0a0, 20, 100 );
  
  const camera = new THREE.PerspectiveCamera( 45, window.innerWidth / window.innerHeight, 0.1, 200 );
  camera.position.set( 10, 10, 10 );
  
  const light1 = new THREE.HemisphereLight( 0xffffff, 0x444444 );
  light1.position.set( 0, 20, 0 );
  scene.add( light1 );
  
  const light = new THREE.DirectionalLight( 0xffffff );
  light.position.set( 0, 20, 10 );
  light.castShadow = true;
  light.shadow.camera.top = 180;
  light.shadow.camera.bottom = - 100;
  light.shadow.camera.left = - 120;
  light.shadow.camera.right = 120;
  scene.add( light );
  
  // ground
  var mesh = new THREE.Mesh(
    new THREE.PlaneBufferGeometry( 200, 200 ),
    new THREE.MeshPhongMaterial({ color: 0x404040, depthWrite: false })
  );
  mesh.rotation.x = - Math.PI / 2;
  mesh.receiveShadow = true;
  scene.add( mesh );
  
  var grid = new THREE.GridHelper(10, 10, 0x000000, 0x000000);
  grid.material.opacity = 0.2;
  grid.material.transparent = true;
  scene.add(grid);
  // model
  var loader = new OBJLoader();
  
  var testMaterial = new THREE.MeshLambertMaterial({color: 0x990000});

  loader.load(
    `/api/model/${modelName}`,
    function ( object ) {
      object.traverse(function (child) {
        if (child.isMesh) {
          console.log(child)
          child.castShadow = true;
          child.receiveShadow = true;
          child.material = testMaterial;
        }
      });

  
      scene.add(object);
    }
  );

  function assignUVs(geometry) {

    geometry.faceVertexUvs[0] = [];

    geometry.faces.forEach(function(face) {

        var components = ['x', 'y', 'z'].sort(function(a, b) {
            return Math.abs(face.normal[a]) > Math.abs(face.normal[b]);
        });

        var v1 = geometry.vertices[face.a];
        var v2 = geometry.vertices[face.b];
        var v3 = geometry.vertices[face.c];

        geometry.faceVertexUvs[0].push([
            new THREE.Vector2(v1[components[0]], v1[components[1]]),
            new THREE.Vector2(v2[components[0]], v2[components[1]]),
            new THREE.Vector2(v3[components[0]], v3[components[1]])
        ]);

    });

    geometry.uvsNeedUpdate = true;
}
  
  var cube = new THREE.Mesh(
    new THREE.BoxGeometry(5, 5, 5),
    testMaterial
  );
  cube.position.set(0, 10, 0);
  scene.add(cube);
  
  console.log(container.outerWidth)
  const renderer = new THREE.WebGLRenderer( { antialias: true } );
  renderer.setPixelRatio( window.devicePixelRatio );
  renderer.setSize( container.offsetWidth, container.offsetHeight );
  renderer.shadowMap.enabled = true;
  container.appendChild( renderer.domElement );
  
  const controls = new OrbitControls( camera, renderer.domElement );
  controls.target.set( 0, 0, 0 );
  controls.update();
  
  const onWindowResize = function () {
    camera.aspect = container.offsetWidth / container.offsetHeight;
    camera.updateProjectionMatrix();
    renderer.setSize( container.offsetWidth, container.offsetHeight );
  }

  window.addEventListener( 'afterLoad', onWindowResize, false );
  window.addEventListener( 'resize', onWindowResize, false );
  
  const animate = function () {
    requestAnimationFrame( animate );
    var delta = clock.getDelta();
    renderer.render( scene, camera );
  }
  
  animate();
}

window.loadModel = loadModel;