using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorManager : MonoBehaviour
{
    private CustomDebugManager _customDebugManager;
    private Texture _myTexture;
    
    private void Awake()
    {
        _customDebugManager = CustomDebugManager.instance;
        //_myTexture = gameObject.GetComponent<Texture>();
    }
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
