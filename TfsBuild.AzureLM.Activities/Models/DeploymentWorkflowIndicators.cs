using TfsBuild.AzureLm.Activities.Enumerations;

namespace TfsBuild.AzureLM.Activities.Models
{
    public class DeploymentWorkflowIndicators
    {
        public DeploymentSlots DeploymentSlot { get; set; }
        public bool DeploymentInWay { get; set; }
        public bool DoDelete { get; set; }
        public bool DoSwap { get; set; }

        public override int GetHashCode()
        {
            return DeploymentSlot.GetHashCode() + DeploymentInWay.GetHashCode() +
                DoDelete.GetHashCode() + DoSwap.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is DeploymentWorkflowIndicators))
            {
                return false;
            }

            var typedObj = (DeploymentWorkflowIndicators) obj;

            return ((typedObj.DeploymentSlot == DeploymentSlot) &&
                (typedObj.DeploymentInWay == DeploymentInWay) && 
                (typedObj.DoDelete == DoDelete) && 
                (typedObj.DoSwap == DoSwap));
        }
    }
}
