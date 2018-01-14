namespace TimeOff.Repository
open TimeOff

type ManagerRepository<'a> = {
    GetAll : unit -> 'a seq
    ValidateTimeOff : TimeOffRequest -> 'a option
    RefuseTimeOff : TimeOffRequest -> 'a option
    ValidateCancelTimeOff : TimeOffRequest -> 'a option
    RefuseCancelTimeOff : TimeOffRequest -> 'a option
}
