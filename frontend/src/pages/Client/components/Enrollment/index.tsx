import _mock from '../../../../assets/mock.png'
import _error from '../../../../assets/x-circle.svg'
import _created from '../../../../assets/check-circle.svg'
import { EnrollmentType } from '../../../../core/types'
import { format, isFuture } from 'date-fns'
import { ptBR } from 'date-fns/locale'
import { currencyDotted } from '../../../../core/script'
import { ModalType } from '../..'

type Props = {
    enrollment: EnrollmentType;
    modalFunc: (value: ModalType) => void;
}

const Enrollment = ({ enrollment, modalFunc }: Props) => {
    const isValid = enrollment?.expireAt && isFuture(new Date(enrollment.expireAt)) ? true : false;;
    const isLoading = enrollment.status === 'PREPARING';
    const hasError = enrollment.status === 'ERROR';

    return <div className={`bg-neutral-50 border border-gray-300 rounded-md grid grid-rows-3 grid-flow-col max-w-lg p-2 relative m-1 ${isLoading ? 'opacity-70' : ''}`}>
        <img className="absolute top-2 right-0 left-16 " src={hasError ? _error : _created} />
        <div className="row-span-3 mr-2">
            <div className='flex items-center justify-center rounded-full w-20 h-20 shadow-md bg-white overflow-hidden'>
                {enrollment.insurer.picture ? <img src={enrollment.insurer.picture} /> : <p className='text-xs font-bold text-center'>{enrollment.insurer.name}</p>}
            </div>
            <div>
                <label className="text-xs text-neutral-400">Rating</label>
                <p className="text-sm text-neutral-900 font-bold">{enrollment.rating ? enrollment.rating : '--'}</p>
            </div>
            <div>
                <label className="text-xs text-neutral-400">Data de cadastro</label>
                <p className="text-sm text-neutral-900 font-bold">{format(new Date(enrollment.createdAt), 'dd/MM/yyyy', { locale: ptBR })}</p>
            </div>
            <div>
                <label className="text-xs text-neutral-400">Data de expiração</label>
                <p className={`text-sm font-bold ${isValid ? 'text-neutral-900' : 'text-rose-700'}`}>{enrollment.expireAt ? format(new Date(enrollment.expireAt), 'dd/MM/yyyy', { locale: ptBR }) : '--'}</p>
            </div>
            <button onClick={() => modalFunc({ value: "DETAILS", show: true, uniqueId: enrollment.insurerId })} className="bg-neutral-300 p-1 mt-2 rounded-lg font-bold text-base text-neutral-500 hover:bg-neutral-200">Gerenciar</button>
        </div>

        {hasError && enrollment.takers.length == 0 ?
            <div className="flex flex-col col-span-2 row-span-3 bg-white">
                <div className="overflow-y-scroll overflow-x-hidden scrollbar flex flex-col text-xl items-center justify-center bg-rose-100 shadow rounded-md p-2 w-96 h-28">
                    <p className="font-bold text-rose-700">Mensagem de erro:</p>
                    <p className="font-neutral-900 max-h-52 p-1">{enrollment.warn}</p>
                </div>
                <div className="mt-2 p-2 shadow-md rounded-md w-96">
                    <h4 className="text-lg text-slate-600">Possível taxa para o cliente</h4>
                    <p className="text-sm">Lembre-se que essa é uma <b>estimativa</b> de quanto seria.</p>
                    <table className="table-fixed">
                        <thead>
                            <tr>
                                <td>Fonte</td>
                                <td className="px-4">Rating</td>
                                <td>Taxa (%)</td>
                            </tr>
                        </thead>
                        <tbody>
                            {enrollment.virtualRates.map(x => (
                                <tr className="text-center">
                                    <td className='py-1'><span className="bg-fuchsia-200 border border-fuchsia-700 rounded-full px-6 py-0.5">{x.ratingSource}</span></td>
                                    <td>{x.rating}</td>
                                    <td><b>{x.virtualRate.toFixed(2)}</b></td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                </div>
            </div>
            :
            <>
                <div className={`shadow p-2 col-span-2 ${hasError ? 'row-span-2 rounded-t-md bg-rose-100' : 'row-span-3 rounded-md bg-white'} w-96`}>
                    <table className="table-auto w-full h-full">
                        <thead>
                            <tr>
                                <th className="w-24">
                                    {isLoading && (
                                        <div className="lds-ellipsis"><div></div><div></div><div></div><div></div></div>
                                    )}
                                </th>
                                <th className="text-sm text-neutral-600">Limite (R$)</th>
                                <th className="text-sm text-neutral-600">Saldo (R$)</th>
                                <th className="text-sm text-neutral-600">Taxa</th>
                            </tr>
                        </thead>
                        <tbody>
                            {enrollment.takers.map(item => (
                                <tr key={item.id} className="text-sm text-neutral-800 even:border-y">
                                    <th>{item.category}</th>
                                    <th>{item.limit ? currencyDotted(item.limit) : '0'}</th>
                                    <th>{item.balance ? currencyDotted(item.balance) : '0'}</th>
                                    <th>{`${String(item.rate).replace('.', ',')}`}</th>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                </div>

                {hasError && (
                    <div className="bg-rose-100 rounded-b-md shadow p-2 text-sm max-h-20 overflow-y-scroll overflow-x-hidden scrollbar col-span-2 w-96">
                        <span className='font-bold text-rose-700'>Mensagem de erro: </span>
                        <span className="font-neutral-900">{enrollment.warn}</span>
                    </div>
                )}
            </>}
    </div>

}

export default Enrollment;