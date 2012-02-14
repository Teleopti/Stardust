


using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.WpfControls.Demo.Models;
using Teleopti.Ccc.WpfControls.Demo.ViewModels;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WpfControls.Demo
{
    public static  class AccountFactory
    {
   
        public static IList<PersonViewModel> CreatePersonsWithPersonAccountModels()
        {
            IList<PersonViewModel> ret = new List<PersonViewModel>();

            DateTime startDateTime = new DateTime(2001,1,1,1,1,0,DateTimeKind.Utc);
           
            for (int i = 0; i < 30; i++)
            {
                Person p = new Person();
                p.Name = new Name("Blah","Blah");
                p.AddPersonAccount(new PersonAccountDay(startDateTime, 10, 3, 0, new DescriptionDayTracker()));
                p.AddPersonAccount(new PersonAccountDay(startDateTime.AddMonths(1), 14, 14, 0, new DescriptionDayTracker()));
                p.AddPersonAccount(new PersonAccountDay(startDateTime.AddMonths(2), 8, 12, 0, new DescriptionDayTracker()));
                p.AddPersonAccount(new PersonAccountDay(startDateTime.AddMonths(3), 16, 7, 4, new DescriptionDayTracker()));
                p.AddPersonAccount(new PersonAccountDay(startDateTime.AddMonths(4), 0, 6, 0, new DescriptionDayTracker()));
                p.AddPersonAccount(new PersonAccountDay(startDateTime.AddMonths(5), 2, 0, 0, new DescriptionDayTracker()));
                ret.Add(new PersonViewModel(p));
            }
            return ret;
            
        }

        public static IList<PersonViewModel> LoadAllPersonsAsViewModels()
        {
            IList<PersonViewModel> models = new List<PersonViewModel>();

            using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                //Läs upp alla personer:
                ICollection<IPerson> people = new Collection<IPerson>();
                PersonRepository personRepository = new PersonRepository(uow);
                people = personRepository.FindAllSortByName();
               
                

                foreach(IPerson person in people)
                {
                    
                    person.AddPersonAccount(new PersonAccountDay(new DateTime(2001,1,1,0,0,0,DateTimeKind.Utc), 10, 3, 0, new DescriptionDayTracker()));
                    person.AddPersonAccount(new PersonAccountDay(new DateTime(2002, 1, 1, 0, 0, 0, DateTimeKind.Utc), 10, 3, 0, new DescriptionDayTracker()));
                    person.AddPersonAccount(new PersonAccountDay(new DateTime(2003, 1, 1, 0, 0, 0, DateTimeKind.Utc), 10, 3, 0, new DescriptionDayTracker()));
                    person.AddPersonAccount(new PersonAccountDay(new DateTime(2004, 1, 1, 0, 0, 0, DateTimeKind.Utc), 10, 3, 0, new DescriptionDayTracker()));
                    person.AddPersonAccount(new PersonAccountDay(new DateTime(2005, 1, 1, 0, 0, 0, DateTimeKind.Utc), 10, 3, 0, new DescriptionDayTracker()));
                    person.AddPersonAccount(new PersonAccountDay(new DateTime(2006, 1, 1, 0, 0, 0, DateTimeKind.Utc), 10, 3, 0, new DescriptionDayTracker()));
                    person.AddPersonAccount(new PersonAccountDay(new DateTime(2007, 1, 1, 0, 0, 0, DateTimeKind.Utc), 10, 3, 0, new DescriptionDayTracker()));
                    person.AddPersonAccount(new PersonAccountDay(new DateTime(2007, 2, 1, 0, 0, 0, DateTimeKind.Utc), 10, 3, 0, new DescriptionDayTracker()));
                    person.AddPersonAccount(new PersonAccountDay(new DateTime(2007, 3, 1, 0, 0, 0, DateTimeKind.Utc), 10, 3, 0, new DescriptionDayTracker()));
                    person.AddPersonAccount(new PersonAccountDay(new DateTime(2007, 4, 1, 0, 0, 0, DateTimeKind.Utc), 10, 3, 0, new DescriptionDayTracker()));
                    person.AddPersonAccount(new PersonAccountDay(new DateTime(2007, 5, 1, 0, 0, 0, DateTimeKind.Utc), 10, 3, 0, new DescriptionDayTracker()));
                    person.AddPersonAccount(new PersonAccountDay(new DateTime(2007, 6, 1, 0, 0, 0, DateTimeKind.Utc), 10, 3, 0, new DescriptionDayTracker()));
                    person.AddPersonAccount(new PersonAccountDay(new DateTime(2008, 6, 1, 0, 0, 0, DateTimeKind.Utc), 10, 3, 0, new DescriptionDayTracker()));
                    person.AddPersonAccount(new PersonAccountDay(new DateTime(2009, 6, 1, 0, 0, 0, DateTimeKind.Utc), 10, 3, 0, new DescriptionDayTracker()));
                    PersonViewModel viewModel = new PersonViewModel(person);

                    models.Add(viewModel);
                }

               

            }
            return models;
        }
    }
}
