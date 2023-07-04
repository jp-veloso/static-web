import _gmailLogo from '../../assets/gmail.png'
import _logo from '../../assets/_logo.svg'
import axios from 'axios';
import qs from 'qs'
import {  useGoogleLogin } from '@react-oauth/google';
import { useNavigate } from 'react-router-dom';
import { User } from '../../core/types';

const Auth = () => {
    const navigate = useNavigate();

    const login = useGoogleLogin({
        onSuccess: async codeResponse => {

            const payload = {
                hd: "grantoseguros.com",
                code: codeResponse.code
            }

            const tokens = await axios.post(`${import.meta.env.VITE_BACKEND_URL}/auth/google`, qs.stringify(payload), { headers: { 'Content-Type': 'application/x-www-form-urlencoded' } })

            if (tokens.status == 200) {
                localStorage.setItem('data', JSON.stringify(tokens.data))
                navigate("/")
            }
        },
        scope: 'openid https://www.googleapis.com/auth/userinfo.profile https://www.googleapis.com/auth/userinfo.email',
        flow: "auth-code"
    })

    /*const adminLogin = async () => {

        const payload = {
            username: 'admin@admin',
            password: 'admin'
        }

        const token = await axios.post(`${import.meta.env.VITE_BACKEND_URL}/auth/login`, qs.stringify(payload), { headers: { 'Content-Type': 'application/x-www-form-urlencoded' }})

        const user : User = {
            token: token.data,
            picture: '',
            name: 'Admin Admin',
            department: 'GERENTE'
        }

        localStorage.setItem('data', JSON.stringify(user))
        navigate("/")
    }*/

    return (
        <div className='auth-background flex justify-center items-center w-full h-screen'>
            <div className='flex flex-col items-center m-5'>
                <img src={_logo}></img>
                <div className='bg-gray-100 rounded-xl shadow-md p-8 flex flex-col items-center justify-center max-w-xl text-center mt-8'>
                    <h1 className='text-4xl font-bold text-gray-900 mb-6'>Bem-vindo ao portal do colaborador</h1>
                    <p className='text-2xl text-gray-500 mb-8'>Para entrar no portal você deve utilizar a sua conta <span className='font-bold text-gray-900'>@grantoseguros</span></p>
                    <button className='auth-button rounded-xl p-4 text-white flex' onClick={() => login()}>
                        <img className='mr-2 mt-0.5' src={_gmailLogo}></img>
                        <span>Entrar com a sua conta gmail</span>
                    </button>
                </div>
                {/*<button className='bg-white mt-6 p-2 rounded-lg' onClick={() => adminLogin()}>ADMIN LOGIN</button>*/}
            </div>
        </div>
    )
}

export default Auth;