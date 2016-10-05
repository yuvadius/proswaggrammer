using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AppleController : MonoBehaviour {

	public GameObject applePrefab;
    private bool CanHaveApples;
	public int maxNumOfApples; int prev1;
	public float ratePerSecond; float prev2;
	float appleHeight;
	private GlobeController globe;
    private static AppleController instance;
    public SphereCollider spherCollider;

	List<Apple> appleList = new List<Apple>();
	List<Apple> applePool = new List<Apple>();

    void Awake()
    {
        if (instance)
            DestroyImmediate(gameObject);
        else
        {
            DontDestroyOnLoad(gameObject);
            instance = this;
        }
    }

	// Use this for initialization
	void Start () {
        globe = GameObject.Find("Game Controllers").GetComponent<GlobeController>();
	}

    public void SetCanHaveApples(bool CanHaveApples)
    {
        this.CanHaveApples = CanHaveApples;
    }
	
	// Update is called once per frame
    public void myUpdate()
    {
        if (CanHaveApples)
        {
            appleHeight = globe.globeRadius;
            if (prev1 != maxNumOfApples)
            {
                prev1 = maxNumOfApples;
            }
            if (prev2 != ratePerSecond)
            {
                prev2 = ratePerSecond;
            }

            if (appleList.Count < maxNumOfApples && PhotonNetwork.isMasterClient)
            {
                if (Time.deltaTime * ratePerSecond > Random.Range(0.0f, 1.0f))
                {
                    //create apple
                    Apple apple = null;
                    if (PhotonNetwork.offlineMode)
                    {
                        if (applePool.Count > 0)
                        {
                            apple = applePool[applePool.Count - 1];
                            applePool.RemoveAt(applePool.Count - 1);
                        }
                        else
                        {
                            apple = (GameObject.Instantiate(applePrefab)).GetComponent<Apple>();
                        }
                        apple.gameObject.name = "Apple";
                        apple.gameObject.SetActive(true);
                    }
                    else
                    {
                        Vector3 randomVector = Apple.AvoidOverlap(appleHeight, spherCollider.radius) * appleHeight;
                        GameObject appleObject = PhotonNetwork.InstantiateSceneObject("Apple", randomVector, Quaternion.LookRotation(randomVector * 100), 0, null);
                        apple = appleObject.GetComponent<Apple>();
                    }
                    appleList.Add(apple);
                }
            }
        }
    }

	public void destroy(Apple apple){
		appleList.Remove(apple);
	}

	public void addAppleToPool(Apple apple){
		applePool.Add(apple);
		apple.gameObject.SetActive(false);
	}

	public void setHeight(float globeHeight){
		appleHeight = globeHeight;
		resetAppleHeight();
	}

	void resetAppleHeight(){
		foreach(Apple apple in appleList){
			apple.setHeight(appleHeight, true);
		}
	}
}
