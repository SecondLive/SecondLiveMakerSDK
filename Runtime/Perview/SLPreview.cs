using SecondLive.Sdk.Sapce;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
namespace SecondLive.Maker
{
    public class SLPreview : MonoBehaviour
    {
        public SL_Space space;
        public SL_SpawnPoint[] spawns;

        private void Awake()
        {
            UnityEditor.SceneManagement.EditorSceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDestroy()
        {
            UnityEditor.SceneManagement.EditorSceneManager.sceneLoaded -= OnSceneLoaded;
        }

        // Start is called before the first frame update
        void Start()
        {
            spawns = GameObject.FindObjectsOfType<SL_SpawnPoint>();
            if (spawns == null || spawns.Length == 0)
            {
                Debug.LogError(Define.Text.FIND_SPAWNPOINT_ERROR);
                return;
            }

            if (Camera.main != null)
            {
                Object.DestroyImmediate(Camera.main.gameObject, false);
            }

            UnityEditor.SceneManagement.EditorSceneManager.LoadSceneInPlayMode("Packages/com.secondlive.maker/PackageResources/Perview/Scene/Play.unity",
                new UnityEngine.SceneManagement.LoadSceneParameters(UnityEngine.SceneManagement.LoadSceneMode.Additive));
        }

        private void OnSceneLoaded(Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
        {
            int index = UnityEngine.Random.Range(0, spawns.Length);
            var spawnpoint = spawns[index];
            var v2 = Random.insideUnitCircle;
            var offset = (spawnpoint.minRadius + (spawnpoint.maxRadius - spawnpoint.minRadius) * v2.magnitude )* v2;
            var position = spawnpoint.transform.position;
            position = new Vector3(position.x + offset.x, position.y, position.z + offset.y);

            var cc = GameObject.FindObjectOfType<CharacterController>();
            cc.transform.position = position;
            cc.transform.rotation = spawnpoint.transform.rotation;
        }
    }
}
#endif
