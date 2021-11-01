using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckInternetConnection : MonoBehaviour
{
	public const int NotReachable = 0;                   // 沒有網路
	public const int ReachableViaLocalAreaNetwork = 1;   // 網路Wifi,網路線。
	public const int ReachableViaCarrierDataNetwork = 2; // 網路3G,4G。

	[SerializeField] GameObject UIimage;

	public static CheckInternetConnection instance;

	private bool needsUpdate;
	private bool connectionStatus;
	private bool connectionSuccess;

	public bool isConnectedInternet;

    private void Awake()
    {
		needsUpdate = true;
		connectionStatus = false;
		connectionSuccess = false;
		isConnectedInternet = true;
	}

    // Use this for initialization
    void Start()
	{
		if (instance != null)
		{
			Destroy(gameObject);
		}
		else
		{
			instance = this;
			DontDestroyOnLoad(gameObject);
		}
	}

    private void Update()
    {
        if (needsUpdate)
        {
			needsUpdate = false;
			connectionStatus = false;
			connectionSuccess = true;
			isConnectedInternet = true;

			// IPhone, Android
			int nStatus = ConnectionStatus();
			//Debug.Log("ConnectionStatus : " + nStatus);
			if (nStatus > 0)
            {
				connectionStatus = true;
				//Debug.Log("有連線狀態");
			}
			else
            {
				connectionStatus = false;
				//Debug.Log("無連線狀態");
			}
			this.StartCoroutine(PingConnect());
		}
    }

    public static int ConnectionStatus()
	{

		int nStatus;

		if (Application.internetReachability == NetworkReachability.NotReachable)
			nStatus = NotReachable;
		else if (Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork)
			nStatus = ReachableViaLocalAreaNetwork;
		else if (Application.internetReachability == NetworkReachability.ReachableViaCarrierDataNetwork)
			nStatus = ReachableViaCarrierDataNetwork;
		else
			nStatus = -1;

		return nStatus;
	}

	IEnumerator PingConnect()
	{
		//Google IP
		string googleTW = "203.66.155.50";
		//YahooTW IP
		string yahooTW = "116.214.12.74";
		//Ping網站
		Ping ping = new Ping(googleTW);

		int nTime = 0;

		while (!ping.isDone)
		{
			yield return new WaitForSeconds(0.1f);

			if (nTime > 30) // time 3 sec, OverTime
			{
				nTime = 0;
				connectionSuccess = false;
				//Debug.Log("連線失敗 : " + ping.time);
				break;
			}
			nTime++;
		}
		yield return ping.time;

		//Debug.Log("連線成功");

		if (connectionSuccess && connectionStatus)
        {
			UIimage.SetActive(false);
			isConnectedInternet = true;
		}
		else
        {
			UIimage.SetActive(true);
			isConnectedInternet = false;
		}

		needsUpdate = true;
	}
}