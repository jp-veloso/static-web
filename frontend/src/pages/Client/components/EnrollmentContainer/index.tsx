import { useCallback, useEffect, useState } from "react"
import { ModalType } from "../.."
import { makePrivateRequest } from "../../../../core/script"
import { EnrollmentType } from "../../../../core/types"
import Enrollment from "../Enrollment"

type Props = {
    clientId: number;
    modalFunc: (value: ModalType) => void;
}

const EnrollmentContainer = ({ clientId, modalFunc }: Props) => {

    const [enrollments, setEnrollments] = useState<EnrollmentType[]>()

    const getEnrollments = useCallback(() => {
        console.log('Buscou matrículas')

        makePrivateRequest({ url: `/clients/${clientId}/enrollments` })
            .then(response => {
                setEnrollments(response.data.data)
            })
    }, [])

    useEffect(() => {
        getEnrollments()

        var counter = 0;
        var looper = setInterval(async () => {
            counter++;

            getEnrollments()

            if (counter >= 5) {
                clearInterval(looper)
            }
        }, 8000)

    }, [getEnrollments])

    const syncEnrollment = (insurers: string = '') => {
        makePrivateRequest({ url: `/clients/${clientId}/enrollments/reload`, method: 'POST', params: { insurers } })
            .then(async () => {
                getEnrollments()

                var counter = 0;
                var looper = setInterval(async () => {
                    counter++;

                    getEnrollments()

                    if (counter >= 7) {
                        clearInterval(looper)
                    }
                }, 8000)
            })
    }

    return (
        <div className="basis-full mr-1">
            <div className="flex justify-end w-full">
                <button onClick={() => modalFunc({ value: "DETAILS", show: true })} className="bg-gradient-to-r from-purple-800 via-violet-900 to-purple-800 rounded-xl p-2 text-white mt-2 hover:opacity-90">Cadastrar seguradora</button>
                <button onClick={() => syncEnrollment()} className="rounded-xl p-2 text-neutral-900 mt-2 bg-neutral-300 hover:bg-neutral-200 ml-3">Sincronizar seguradoras</button>
            </div>
            <div className="flex flex-wrap justify-center items-center m-2 border-rose-600">
                {enrollments && (
                    enrollments.map(item => (
                        <Enrollment modalFunc={modalFunc} key={`${item.insurer.id}-${clientId}`} enrollment={item} />
                    ))
                )}
            </div>
        </div>
    )
}

export default EnrollmentContainer;