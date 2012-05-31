//using System;
//using Teleopti.Ccc.Infrastructure.Repositories;
//using Teleopti.Interfaces.Infrastructure;

//namespace Teleopti.Ccc.Sdk.ServiceBus.Denormalizer
//{
//    public class UpdateGroupingReadModel : IUpdateGroupingReadModel 
//    {
//        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
//        private readonly IGroupingReadOnlyRepository _groupingReadOnlyRepository;

//        public UpdateGroupingReadModel(IUnitOfWorkFactory unitOfWorkFactory, IGroupingReadOnlyRepository groupingReadOnlyRepository)
//        {
//            _unitOfWorkFactory = unitOfWorkFactory;
//            _groupingReadOnlyRepository = groupingReadOnlyRepository;
//        }

//        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2")]
//        public void Execute(int type,Guid[] ids)
//        {
//            using (_unitOfWorkFactory.CreateAndOpenStatelessUnitOfWork())
//            {
//                if (type == 1)
//                {
//                    _groupingReadOnlyRepository.UpdateGroupingReadModel(ids);
//                }
//                if (type == 2)
//                {
//                    _groupingReadOnlyRepository.UpdateGroupingReadModelGroupPage(ids);
//                }
//                if (type == 3)
//                {
//                    _groupingReadOnlyRepository.UpdateGroupingReadModelData(ids);
//                }
//            }
//        }
//    }
//}
