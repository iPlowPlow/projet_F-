namespace TimeOff.Repositories
open TimeOff

type ManagerRepository<'a> = {
    GetAll : unit -> 'a seq
    ValidateTimeOff : TimeOffRequest -> 'a
    RefuseTimeOff : TimeOffRequest -> 'a
    ValidateCancelTimeOff : TimeOffRequest -> 'a
    RefuseCancelTimeOff : TimeOffRequest -> 'a
}
