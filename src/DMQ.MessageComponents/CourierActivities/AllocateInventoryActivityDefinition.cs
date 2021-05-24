namespace DMQ.MessageComponents.CourierActivities
{
    public class AllocateInventoryActivityDefinition :
        MassTransit.Definition.ActivityDefinition<AllocateInventoryActivity, IAllocateInventoryArguments, IAllocateInventoryLog>
    {
        public AllocateInventoryActivityDefinition()
        {
            ConcurrentMessageLimit = 10;
        }
    }

}
