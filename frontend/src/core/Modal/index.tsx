type Props = {
    children: React.ReactNode;
    title: string;
    buttonTitle: string;
    cancel: () => void;
}

const Modal = ({ children, title, buttonTitle, cancel }: Props) => {

    return <div className="bg-white rounded-lg shadow p-5 relative m-2">
        <h1 className="text-2xl text-neutral-900 font-bold mt-2 mb-4">{title}</h1>
        {children}
        <input type="submit" className="auth-button rounded-xl p-4 text-white w-full cursor-pointer" value={buttonTitle} />
        <button onClick={() => cancel()} type="button" className="absolute top-1 right-1 text-neutral-900 bg-transparent hover:bg-gray-200 hover:text-gray-900 rounded-lg text-sm p-1.5 inline-flex items-center dark:hover:bg-violet-300 dark:hover:text-white">
            <svg aria-hidden="true" className="w-5 h-5" fill="currentColor" viewBox="0 0 20 20" xmlns="http://www.w3.org/2000/svg"><path fillRule="evenodd" d="M4.293 4.293a1 1 0 011.414 0L10 8.586l4.293-4.293a1 1 0 111.414 1.414L11.414 10l4.293 4.293a1 1 0 01-1.414 1.414L10 11.414l-4.293 4.293a1 1 0 01-1.414-1.414L8.586 10 4.293 5.707a1 1 0 010-1.414z" clipRule="evenodd"></path></svg>
        </button>
    </div>

}


export default Modal;