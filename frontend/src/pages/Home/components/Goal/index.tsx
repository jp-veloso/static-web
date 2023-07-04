import _flag from '../../../../assets/flag.svg'
import _check from '../../../../assets/badge-check.svg'
import _fire from '../../../../assets/fire.png'
import { Kpis } from '../../../../core/types'

type Props = {
    kpis: Kpis
}

const Goal = ( { kpis }: Props) => {
    const percentage = Math.round(kpis.clients / 5000 * 100)

    return <div className="bg-white shadow mt-9 mx-6 mb-6 rounded-xl p-8">
        <h1 className="text-center"><img src={_fire} className="inline mb-5" /><span className="text-4xl font-semibold">Meta anual</span></h1>
        <div className="flex justify-evenly mb-24">
            <div className="bg-neutral-100 shadow-md p-6 rounded-md text-center">
                <p className="text-2xl">Atualmente estamos com <br /><span className="font-semibold text-4xl">{kpis.clients}</span> clientes</p>
            </div>
            <div className="bg-neutral-100 shadow-md p-6 rounded-md text-center">
                <p className="text-2xl">Precisamos atingir <br /><span className="font-semibold text-4xl">5000</span> clientes</p>
            </div>
        </div>

        <div className="relative border border rounded-md h-11 ">
            <div className='absolute top-0 right-0 bg-green-600 rounded-r-md h-full w-2 border border-green-500'></div>
            <img className='absolute top-[-27px] right-[-3px]' style={{ left: `${percentage}%` }} src={_flag} />
            <img className='absolute top-[-27px] right-[-3px]' src={_check} />
            <div className="rounded-l-md h-full" style={{ width: `${percentage}%`, background: 'linear-gradient(180deg, #6735BE 0%, #4803C2 100%)' }}></div>
        </div>

        <div className='mt-2 flex'>
            <div className="mr-2 flex">
                <div className="rounded-full bg-violet-900 w-4 h-4 mt-1 mr-1"></div> Onde estamos
            </div>
            <div className="mr-2 flex">
                <div className="rounded-full bg-green-500 w-4 h-4 mt-1 mr-1"></div> Onde chegar
            </div>
        </div>

    </div>
}

export default Goal;