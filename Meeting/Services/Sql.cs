using System;
namespace Meeting.Services
{
    public class Sql
    {
        public static string sqlGetScheduleList = @"select s.ID, s.User, DATE_FORMAT(s.Start,'%H:%i') As Start, 
            DATE_FORMAT(s.End,'%H:%i') As End, s.Room
            from schedule as s, user as u
            where s.User = u.Name and DATE_FORMAT(Start, '%Y-%m-%d') = '@Date'
            order by Start;";

        public static string sqlGetScheduleByRoom = @"select DATE_FORMAT(Start,'%H:%i') As Start,
            DATE_FORMAT(End,'%H:%i') As End
            from schedule
            where Room = '@Room' and DATE_FORMAT(Start, '%Y-%m-%d') = '@Date'
            order by Start;";

        public static string sqlSetSchedule = @"insert into schedule(User, Start, End, Room)
            values
            ('@User','@Start','@End', '@Room');";  
    }
}
