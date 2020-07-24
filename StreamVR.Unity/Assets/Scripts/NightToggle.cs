using LMAStudio.StreamVR.Unity.Logic;
using LMAStudio.StreamVR.Unity.Scripts;
using UnityEngine;
using UnityEngine.XR;

namespace Assets.Scripts
{
    public class NightToggle: MonoBehaviour
    {
        public XRNode inputSource;
        public Light lightSource;
        public Material daytimeSkybox;
        public Material nighttimeSkybox;

        private bool isNight = false;
        private bool clicking = true;

        void Update()
        {
            InputDevice device = InputDevices.GetDeviceAtXRNode(inputSource);
            device.TryGetFeatureValue(CommonUsages.primaryButton, out bool clicked);

            if (clicked)
            {
                if (!clicking)
                {
                    clicking = true;
                    this.ToggleDayNight();
                }
            }
            else
            {
                clicking = false;
            }
        }

        public void ToggleDayNight()
        {
            if (isNight)
            {
                this.MakeDay();
            }
            else
            {
                this.MakeNight();
            }
        }

        public void MakeNight()
        {
            this.isNight = true;

            lightSource.intensity = 0.1f;
            lightSource.color = new Color(
                214 / 255f,
                223 / 255f,
                255 / 255f
            );

            RenderSettings.skybox = nighttimeSkybox;

            var lights = FamilyInstanceLibrary.GetAllLights();
            foreach(GameObject l in lights)
            {
                var geoController = l.GetComponent<FamilyGeometryController>();
                l.GetComponent<MeshRenderer>().material = geoController.NighttimeMaterial;
            }
        }

        public void MakeDay()
        {
            this.isNight = false;

            lightSource.intensity = 1;
            lightSource.color = new Color(
                244 / 255f,
                244 / 255f,
                214 / 255f
            );

            RenderSettings.skybox = daytimeSkybox;

            var lights = FamilyInstanceLibrary.GetAllLights();
            foreach (GameObject l in lights)
            {
                var geoController = l.GetComponent<FamilyGeometryController>();
                l.GetComponent<MeshRenderer>().material = geoController.DaytimeMaterial;
            }
        }
    }
}
