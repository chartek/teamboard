using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TeamBoard.Model;
using Microsoft.TeamFoundation.Client;
using System.Net;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using TfsWorkItem = Microsoft.TeamFoundation.WorkItemTracking.Client.WorkItem;
using BoardWorkItem = TeamBoard.Model.WorkItem;

namespace TeamBoard.Services
{
	public class WorkItemService
	{

		private IConfiguration _config;
    public WorkItemService(IConfiguration config)
		{
			_config = config;
		}

		public IList<BoardWorkItem> GetWorkItems(string projectName)
		{
			var workItems = new List<BoardWorkItem>();

			using (var tfs = GetServer())
			{
				tfs.EnsureAuthenticated();
				var workItemStore = (WorkItemStore)tfs.GetService(typeof(WorkItemStore));

				var workItemMappings = _config.WorkItemMappings[projectName];
				if (workItemMappings == null)
					return workItems;

				Query query = new Query(projectName, workItemMappings);
				query.AddColumn("Id");
				query.AddColumn("Summary");
				query.AddColumn("Description");
				query.AddColumn("Priority");

				foreach (TfsWorkItem
					item in workItemStore.Query(query.ToString()))
				{
					int priority;
					if (!Int32.TryParse(item.Fields[workItemMappings["Priority"]].Value.ToString(), out priority))
						priority = 0;

					workItems.Add(new BoardWorkItem()
						{
							Summary = item.Title,
							Description = item.Description,
							Id = item.Id.ToString(),
							Priority = priority
						});
				}

				return workItems;
			}
		}

		public void Update(BoardWorkItem workItem)
		{
			using (var tfs = GetServer())
			{
				tfs.EnsureAuthenticated();

				var workItemStore = (WorkItemStore)tfs.GetService(typeof(WorkItemStore));

				var workItemMappings = _config.WorkItemMappings[workItem.ProjectName];

				var tfsWorkItem = workItemStore.GetWorkItem(Convert.ToInt32(workItem.Id));
				tfsWorkItem.Fields[workItemMappings["Priority"]].Value = workItem.Priority;
				tfsWorkItem.Fields[workItemMappings["Summary"]].Value = workItem.Summary;
				tfsWorkItem.Fields[workItemMappings["Description"]].Value = workItem.Description;
				tfsWorkItem.Fields[workItemMappings["Id"]].Value = Convert.ToInt32(workItem.Id);

				tfsWorkItem.Save();
			}
		}



		private TeamFoundationServer GetServer()
		{
			return new TeamFoundationServer(_config.TeamFoundationServerName,
							new NetworkCredential(_config.Login, _config.Password, _config.Domain));
		}
	}
}
