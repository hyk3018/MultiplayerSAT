using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using HelloWorld.Shared.Relay;
using TK.Core.Common;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using Unity.Services.Relay;
using UnityEngine;

public class RelayConnectionManager : Singleton<RelayConnectionManager>
{
    [SerializeField]
    private string environment = "production";
    
    [SerializeField]
    private int maxConnections = 2;

    public bool IsRelayEnabled =>
        Transport != null && Transport.Protocol == UnityTransport.ProtocolType.RelayUnityTransport;

    public UnityTransport Transport => NetworkManager.Singleton.gameObject.GetComponent<UnityTransport>();

    public async Task<RelayHostData> SetupRelay()
    {
        await InitialiseUnityService();
        await EnsureUserAuthentication();

        // Always authenticate users
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        // Ask Unity services to allocate a relay server with connection size
        var allocation = await Unity.Services.Relay.Relay.Instance.CreateAllocationAsync(maxConnections);
        
        // Populate host data for return
        var data = new RelayHostData
        {
            Key  = allocation.Key,
            Port = (ushort) allocation.RelayServer.Port,
            AllocationID = allocation.AllocationId,
            AllocationIDBytes = allocation.AllocationIdBytes,
            ConnectionData = allocation.ConnectionData,
            IPv4Address = allocation.RelayServer.IpV4
        };
        
        // Retrieve join code for clients
        data.JoinCode = await Unity.Services.Relay.Relay.Instance.GetJoinCodeAsync(data.AllocationID);
        Debug.Log("Join code is " + data.JoinCode);

        Transport.SetRelayServerData(data.IPv4Address, data.Port, data.AllocationIDBytes,
            data.Key, data.ConnectionData);
        
        return data;
    }

    public async Task<RelayJoinData> JoinRelay(string joinCode)
    {
        await InitialiseUnityService();
        await EnsureUserAuthentication();

        var allocation = await Relay.Instance.JoinAllocationAsync(joinCode);

        var data = new RelayJoinData
        {
            Key = allocation.Key,
            Port = (ushort) allocation.RelayServer.Port,
            AllocationID = allocation.AllocationId,
            AllocationIDBytes = allocation.AllocationIdBytes,
            ConnectionData = allocation.ConnectionData,
            HostConnectionData = allocation.HostConnectionData,
            IPv4Address = allocation.RelayServer.IpV4,
            JoinCode = joinCode
        };
        
        Transport.SetRelayServerData(data.IPv4Address, data.Port, data.AllocationIDBytes, 
            data.Key, data.ConnectionData, data.HostConnectionData);

        return data;
    }

    private static async Task EnsureUserAuthentication()
    {
        // Always authenticate users
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
    }

    private async Task InitialiseUnityService()
    {
        // Set environment options for Unity service
        InitializationOptions options = new InitializationOptions()
            .SetEnvironmentName(environment);

        await UnityServices.InitializeAsync(options);
    }
}
