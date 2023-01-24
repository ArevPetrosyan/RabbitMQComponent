using Microsoft.Extensions.Options;
using RabbitMQComponent.Interfaces;
using RabbitMQComponent.Models.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMQComponent.Services
{
    public class ConfigDataService : IConfigDataService
    {
        private readonly FeedConstructModel feedData;
        private readonly TargetProxyModel targetData;

        public ConfigDataService(IOptions<FeedConstructModel> _feedData, IOptions<TargetProxyModel> _targetData)
        {
            feedData = _feedData.Value;
            targetData = _targetData.Value;
        }

        public FeedConstructModel FeedConstructModel => feedData;
        public TargetProxyModel TargetProxyModel => targetData;
    }
}
