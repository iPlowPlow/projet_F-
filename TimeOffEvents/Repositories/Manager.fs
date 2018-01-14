namespace TimeOff.Repositories
open TimeOff

type ManagerRepository<'a> = {
    GetAll : unit -> 'a seq
    CreateTimeOff : TimeOffRequest -> 'a
    ValidateTimeOff : TimeOffRequest -> 'a
}

