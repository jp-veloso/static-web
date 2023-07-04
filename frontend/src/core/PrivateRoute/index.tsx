import { isAllowedByRole, isAuthenticated } from "../script";
import { Navigate, Outlet } from "react-router-dom";

type Props = {
    allowedRoles?: string[]
}

const PrivateRoute = ({ allowedRoles }: Props) => {
    if(!isAuthenticated()){
        return <Navigate to={'/login'} replace/>
    }

    if(isAuthenticated() && !isAllowedByRole(allowedRoles)){
        return <Navigate to={'/'} replace/>
    }

    return <Outlet/>;
}

export default PrivateRoute;