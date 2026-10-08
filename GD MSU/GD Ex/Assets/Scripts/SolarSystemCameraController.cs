using UnityEngine;
using UnityEngine.UI;

public class SolarSystemCameraController : MonoBehaviour
{
    public Transform[] targets;
    public Text selectionLabel;
    public Vector3 followOffset = new Vector3(0f, 4.5f, -9f);
    public float followSmoothing = 3f;

    private Transform selectedTarget;

    private void Start()
    {
        transform.position = new Vector3(0f, 19f, -29f);
        transform.LookAt(Vector3.zero);
        SetLabel("SOLAR SYSTEM");
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                PlanetTarget planet = hit.collider.GetComponent<PlanetTarget>();
                if (planet != null)
                {
                    selectedTarget = planet.transform;
                    SetLabel(planet.displayName);
                }
            }
        }

        if (selectedTarget == null)
        {
            return;
        }

        Vector3 desiredPosition = selectedTarget.position + followOffset;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, followSmoothing * Time.deltaTime);
        Quaternion desiredRotation = Quaternion.LookRotation(selectedTarget.position - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, followSmoothing * Time.deltaTime);
    }

    private void SetLabel(string value)
    {
        if (selectionLabel != null)
        {
            selectionLabel.text = value;
        }
    }
}