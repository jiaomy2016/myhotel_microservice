﻿using Consul;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace common.libs
{
    public class ServiceInfo
    {
        public string Name { get; set; }

        public string IP { get; set; }

        public int Port { get; set; }

        public string HealthCheckAddress { get; set; }
    }

    public class ConsulRegister : IHostedService
    {
        IConsulClient _consulClient;
        string _serviceID = "";
        ServiceInfo _serviceInfo;
        public ConsulRegister(IConfiguration config, IConsulClient consulClient)
        {
            _serviceInfo = new ServiceInfo();
            var sc = config.GetSection("serviceInfo");
            _serviceInfo.Name = sc["name"];
            _serviceInfo.IP = sc["ip"];
            _serviceInfo.HealthCheckAddress = sc["HealthCheckAddress"];
            _serviceInfo.Port = int.Parse(sc["Port"]);

            _serviceID = $"{_serviceInfo.Name}={_serviceInfo.IP}:{_serviceInfo.Port}";
            _consulClient = consulClient;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("start to register service info to consul client ...");
            await _consulClient.Agent.ServiceRegister(new AgentServiceRegistration
            {
                ID = _serviceID,
                Name = _serviceInfo.Name,// 服务名
                Address = _serviceInfo.IP, // 服务绑定IP
                Port = _serviceInfo.Port, // 服务绑定端口
                Check = new AgentServiceCheck()
                {
                    DeregisterCriticalServiceAfter = TimeSpan.FromSeconds(1),//服务启动多久后注册
                    Interval = TimeSpan.FromSeconds(5),//健康检查时间间隔
                    HTTP = $"http://{_serviceInfo.IP}:{_serviceInfo.Port}/" + _serviceInfo.HealthCheckAddress,//健康检查地址
                    Timeout = TimeSpan.FromSeconds(5)
                }
            });
            Console.WriteLine("register service info to consul client Successful ...");
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _consulClient.Agent.ServiceDeregister(_serviceID);
            Console.WriteLine("Deregister service info from consul client Successful ...");
        }
    }
}
