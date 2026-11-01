using UnityEngine;
using System.Collections;

public class Flare_01 : MonoBehaviour {
//--------------------------------------------------------------------------------------------------
//--------------------------------------------------------------------------------------------------
//--------------------------------------------------------------------------------------------------

    public bool cam_orientation = false;
    public bool belong_to_scene_light = false;
    public bool stroboskob_fx = false;

    public float flare_intensity = 1.0f;

    public float fade_distance = 50.0f;
    public float max_distance = 100.0f;

    //--------------------------------
    float distance_intensity;

    float flare_xrot;
    float flare_zrot;

    Color flare_color;
    float last_frame_flare_color;

    float strobo_interval;
    float strobo_count;
    bool  strobo_mode;

    float max_dist_quad;
    float fade_dist_quad;

    bool last_frame_flare_enabled;

    AirfieldLights befeuerungen;

//--------------------------------------------------------------------------------------------------
//--------------------------------------------------------------------------------------------------
//--------------------------------------------------------------------------------------------------
//
// Init 
//
//--------------------------------------------------------------------------------------------------
//--------------------------------------------------------------------------------------------------
//--------------------------------------------------------------------------------------------------
void Start() 
{
    GetComponent<Renderer>().enabled = false; 

    last_frame_flare_enabled = false;

    flare_color = GetComponent<Renderer>().material.GetColor("_TintColor");
    last_frame_flare_color = 0.0f;

    max_dist_quad = max_distance * max_distance;
    fade_dist_quad = fade_dist_quad * fade_dist_quad;

    flare_xrot = gameObject.transform.eulerAngles.x;
    flare_zrot = gameObject.transform.eulerAngles.z;

    strobo_mode = false;
    strobo_interval = ((UnityEngine.Random.value) * 0.8f) + 1.5f;
    strobo_count = 0.0f;

    befeuerungen = GameObject.Find("AirfieldLights").GetComponent<AirfieldLights>();

}
//--------------------------------------------------------------------------------------------------
//--------------------------------------------------------------------------------------------------
//--------------------------------------------------------------------------------------------------
//
// Update
//
//--------------------------------------------------------------------------------------------------
//--------------------------------------------------------------------------------------------------
//--------------------------------------------------------------------------------------------------
void Update()
{
    //-- wenn flare zur szenen-beleuchtung geh�rt testen ob diese an oder aus ist
    if((belong_to_scene_light == true) && (befeuerungen.airfieldLightsOn == false))
    {
        if(last_frame_flare_enabled == true)
            GetComponent<Renderer>().enabled = false;  
        last_frame_flare_enabled = false;
        return;
    }

    //-- calculate viewport distance 
    float distance = (transform.position - Camera.main.transform.position).sqrMagnitude;

    if(distance > max_dist_quad)
    {
        if(last_frame_flare_enabled == true)
            GetComponent<Renderer>().enabled = false;  
        last_frame_flare_enabled = false;
        return;
    }
    if(last_frame_flare_enabled == false)
        GetComponent<Renderer>().enabled = true;    
    last_frame_flare_enabled = true;

    //-- test for fade distance
    if(distance > fade_dist_quad)
        distance_intensity = 1.0f - ((distance - fade_dist_quad) / (max_dist_quad - fade_dist_quad));
    else
        distance_intensity = 1.0f;

    //-- set cam orientation
    if(cam_orientation == true)
        gameObject.transform.eulerAngles = new Vector3 (flare_xrot, (Camera.main.transform.eulerAngles.y + 90.0f), flare_zrot);

    //-- do strobo fx
    if(stroboskob_fx == true)
    {
        //-- pause interval
        if(strobo_mode == false)    
        {
            strobo_count += Time.deltaTime * 1.5f;               
            if(strobo_count >= strobo_interval)
            {
                strobo_count = 2.0f;
                strobo_mode = true;
                distance_intensity = 1.0f;
            }
            else
                distance_intensity = 0.0f;
        }
        //-- flash down
        else if(strobo_mode == true)
        {
            strobo_count -= Time.deltaTime * 20.0f;                             
            if(strobo_count <= 0.0f)
            {
                strobo_count = 0.0f;
                strobo_mode = false;
            }
            distance_intensity *= strobo_count;
        }
    }

    //-- set flare intensity
    flare_color.a = flare_intensity * distance_intensity;
    if(flare_color.a != last_frame_flare_color)
    {
        gameObject.transform.GetComponent<Renderer>().material.SetColor("_TintColor", flare_color);
        last_frame_flare_color = flare_color.a;
    }
}
//--------------------------------------------------------------------------------------------------
//--------------------------------------------------------------------------------------------------
//--------------------------------------------------------------------------------------------------
}
