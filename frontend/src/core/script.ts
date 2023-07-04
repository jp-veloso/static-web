import axios, { AxiosRequestConfig } from "axios";
import jwtDecode from "jwt-decode";
import { createContext } from "react";
import { router } from "../main";
import { AccessToken, User } from "./types";

const BASE_URL = import.meta.env.VITE_BACKEND_URL;

export type AsideContextType = {
    isOpen: boolean;
    setContext: (value: boolean) => void;
}
  
export const AsideContext = createContext<AsideContextType>({ isOpen: true, setContext: (value) => null })

//

axios.interceptors.response.use((response) => {return response}, (error) => {
    if(error.response.status === 401){
        router.navigate('/login')
        logout()
    }
    return Promise.reject(error);
})

const makeRequest = (params: AxiosRequestConfig) => {
    return axios({...params, baseURL: BASE_URL})
}

const recoverToken = () => {
    const user = getUser()

    try {
        return jwtDecode(user.token) as AccessToken;
    } catch (error) {
        return { } as AccessToken
    }
}


export function formatCurrency(number: number){
    return number.toLocaleString('pt-br',{style: 'currency', currency: 'BRL'});
}

export function currencyDotted(number: number){
    return number.toLocaleString('pt-br',{style: 'currency', currency: 'BRL'}).replace("R$", "");
}

export const makePrivateRequest = (params: AxiosRequestConfig) => {
    const user = getUser()
    const headers = { Authorization: `Bearer ${user.token}` }
    return makeRequest({...params, headers});
}

export const isAllowedByRole = (auth: string[] = []) => {

    if (auth.length === 0){ 
        return true;
    }

    const { roles } = recoverToken()

    return auth.some(r => roles.includes(r));
}



export const isAuthenticated = () => {
    const token = recoverToken();

    if(Object.keys(token).length === 0){
        return false;
    }

    const { exp } = token;

    return Date.now() <= exp * 1000
}

export const logout = () => {
    localStorage.removeItem('data')
    router.navigate('/login')
}

export const getUser = () => {
    const json = localStorage.getItem('data')
    if(json == null) return { } as User
    return JSON.parse(json) as User    
}

export const getUserDetailed = () => {
    try {
        const user = getUser()
        const tokenDecoded = jwtDecode(user.token) as AccessToken
        user.department = tokenDecoded.dep
        return user;
    } catch(error){
        logout()
    }
}


